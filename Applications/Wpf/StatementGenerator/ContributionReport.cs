// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using IronPdf;
using IronPdf.Threading;

using Newtonsoft.Json;

using RestSharp;

using Rock.Apps.StatementGenerator.RestSharpRequests;
using Rock.Client;
using Rock.Client.Enums;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    ///
    /// </summary>
    public class ContributionReport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContributionReport"/> class.
        /// </summary>
        public ContributionReport( Rock.Client.FinancialStatementGeneratorOptions options )
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public Rock.Client.FinancialStatementGeneratorOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the record count.
        /// </summary>
        /// <value>
        /// The record count.
        /// </value>
        private int RecordCount { get; set; }

        private static long _recordsCompleted = 0;

        /// <summary>
        /// Runs the report returning the number of statements that were generated
        /// </summary>
        /// <returns></returns>
        public int RunReport()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            UpdateProgress( "Connecting..." );

            // Login and setup options for REST calls
            RockConfig rockConfig = RockConfig.Load();

            var restClient = new RestClient( rockConfig.RockBaseUrl );
            restClient.CookieContainer = new System.Net.CookieContainer();
            var rockLoginRequest = new RockLoginRequest( rockConfig.Username, rockConfig.Password );
            var rockLoginResponse = restClient.Execute( rockLoginRequest );

            // Get Selected FinancialStatementTemplate
            var getFinancialStatementTemplatesRequest = new RestRequest( $"api/FinancialStatementTemplates/{this.Options.FinancialStatementTemplateId ?? 0}" );
            var getFinancialStatementTemplatesResponse = restClient.Execute<Client.FinancialStatementTemplate>( getFinancialStatementTemplatesRequest );
            if ( getFinancialStatementTemplatesResponse.ErrorException != null )
            {
                throw getFinancialStatementTemplatesResponse.ErrorException;
            }

            Rock.Client.FinancialStatementTemplate financialStatementTemplate = getFinancialStatementTemplatesResponse.Data;

            var reportSettings = JsonConvert.DeserializeObject<Rock.Client.FinancialStatementTemplateReportSettings>( financialStatementTemplate.ReportSettingsJson );

            // Get Recipients from Rock REST Endpoint
            List<FinancialStatementGeneratorRecipient> recipientList = GetRecipients( restClient );
            string reportRockStatementGeneratorTemporaryDirectory = GetRockStatementGeneratoryTemporaryDirectory( rockConfig );

            this.RecordCount = recipientList.Count;
            _recordsCompleted = 0;

            var tasks = new List<Task>();

            var generatePdfTimingsMS = new ConcurrentBag<double>();

            List<StatementGeneratorRecipientPdfResult> statementGeneratorRecipientPdfResults = new List<StatementGeneratorRecipientPdfResult>();

            IronPdf.Installation.TempFolderPath = Path.Combine( reportRockStatementGeneratorTemporaryDirectory, "IronPdf" );
            Directory.CreateDirectory( IronPdf.Installation.TempFolderPath );

            var tempStatementsFolder = Path.Combine( reportRockStatementGeneratorTemporaryDirectory, "Statements" );
            Directory.CreateDirectory( tempStatementsFolder );

            Task lastTask = null;
            UpdateProgress( "Getting Statements..." );
            foreach ( var recipient in recipientList )
            {
                FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = GetFinancialStatementGeneratorRecipientResult( restClient, recipient );

                var statementGeneratorPdfResult = new StatementGeneratorRecipientPdfResult( financialStatementGeneratorRecipientResult );
                statementGeneratorRecipientPdfResults.Add( statementGeneratorPdfResult );

                if ( string.IsNullOrWhiteSpace( financialStatementGeneratorRecipientResult.Html ) )
                {
                    // don't generate a statement if no statement HTML
                    statementGeneratorPdfResult.PdfTempFileName = null;
                    continue;
                }

                var html = financialStatementGeneratorRecipientResult.Html;
                var footerHtml = financialStatementGeneratorRecipientResult.FooterHtml;

                Stopwatch waitForLastTask = Stopwatch.StartNew();

                // We were able to fetch the HTML for the next state while waiting for the PDF task to finish,
                // but it'll lock up if we don't wait for it to complete
                lastTask?.Wait();

                Debug.WriteLine( $"{waitForLastTask.Elapsed.TotalMilliseconds}ms, waitForLastTask" );

                bool useUniversalRenderJob = true;

                lastTask = Task.Run( () =>
                {
                    var pdfOptions = GetPdfPrintOptions( reportSettings.PDFObjectSettings, financialStatementGeneratorRecipientResult );
                    statementGeneratorPdfResult.PdfTempFileName = Path.Combine( tempStatementsFolder, $"GroupId_{financialStatementGeneratorRecipientResult.GroupId}_PersonID_{financialStatementGeneratorRecipientResult.PersonId}.pdf" );

                    Stopwatch generatePdfStopWatch = Stopwatch.StartNew();

                    IronPdf.PdfDocument pdfDocument;

                    if ( useUniversalRenderJob )
                    {
                        UniversalRenderJob universalRenderJob = new UniversalRenderJob();
                        universalRenderJob.Options = pdfOptions;
                        universalRenderJob.Html = statementGeneratorPdfResult.StatementGeneratorRecipientResult.Html;
                        var pdfBytes = universalRenderJob.DoRemoteRender();
                        pdfDocument = new PdfDocument( pdfBytes );
                    }
                    else
                    {
                        pdfDocument = HtmlToPdf.StaticRenderHtmlAsPdf( statementGeneratorPdfResult.StatementGeneratorRecipientResult.Html, pdfOptions );
                    }

                    generatePdfStopWatch.Stop();

                    Stopwatch savePdfStopWatch = Stopwatch.StartNew();

                    tasks.Add( Task.Run( () =>
                    {
                        recipient.RenderedPageCount = pdfDocument.PageCount;
                        recipient.IsComplete = true;
                        pdfDocument.SaveAs( statementGeneratorPdfResult.PdfTempFileName );
                    } ) );

                    Debug.WriteLine( $"{savePdfStopWatch.Elapsed.TotalMilliseconds} ms, savePdfStopWatch Avg" );

                    var recordsCompleted = Interlocked.Increment( ref _recordsCompleted );

                    if ( recordsCompleted > 2 )
                    {
                        generatePdfTimingsMS.Add( generatePdfStopWatch.Elapsed.TotalMilliseconds );
                        Debug.WriteLine( $"{generatePdfStopWatch.Elapsed.TotalMilliseconds} ms, generatePdf" );
                        Debug.WriteLine( $"{generatePdfTimingsMS.Average()} ms, generatePdf Avg" );
                    }

                    Debug.WriteLine( $"_recordsCompleted:{recordsCompleted}\n" );
                } );

                tasks.Add( lastTask );

                tasks = tasks.Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();

                UpdateProgress( "Processing..." );

                SaveRecipientListStatus( reportRockStatementGeneratorTemporaryDirectory, recipientList );
            }

            SaveRecipientListStatus( reportRockStatementGeneratorTemporaryDirectory, recipientList );

            Task.WaitAll( tasks.ToArray() );

            UpdateProgress( "Creating PDF..." );

            statementGeneratorRecipientPdfResults = statementGeneratorRecipientPdfResults.Where( a => a.PdfTempFileName != null ).ToList();
            this.RecordCount = statementGeneratorRecipientPdfResults.Count();

            _recordsCompleted = 0;


            foreach ( var financialStatementReportConfiguration in this.Options.ReportConfigurationList )
            {

                WriteStatementPDFs( financialStatementReportConfiguration, statementGeneratorRecipientPdfResults );
            }

            UpdateProgress( "Complete" );

            stopWatch.Stop();
            var elapsedSeconds = stopWatch.ElapsedMilliseconds / 1000;
            Debug.WriteLine( $"{elapsedSeconds:n0} seconds" );
            Debug.WriteLine( $"{RecordCount:n0} statements" );
            if ( RecordCount > 0 )
            {
                Debug.WriteLine( $"{( stopWatch.ElapsedMilliseconds / RecordCount ):n0}ms per statement" );
            }

            return this.RecordCount;
        }

        /// <summary>
        private void SaveRecipientListStatus( string reportRockStatementGeneratorTemporaryDirectory, List<FinancialStatementGeneratorRecipient> recipientList )
        {
            var recipientListJson = Newtonsoft.Json.JsonConvert.SerializeObject( recipientList );
            var recipientListJsonFileName = Path.Combine( reportRockStatementGeneratorTemporaryDirectory, "RecipientData.Json" );
            File.WriteAllText( recipientListJsonFileName, recipientListJson );
        }

        private FinancialStatementGeneratorRecipientResult GetFinancialStatementGeneratorRecipientResult( RestClient restClient, FinancialStatementGeneratorRecipient recipient )
        {
            FinancialStatementGeneratorRecipientRequest financialStatementGeneratorRecipientRequest = new FinancialStatementGeneratorRecipientRequest()
            {
                FinancialStatementGeneratorOptions = this.Options,
                FinancialStatementGeneratorRecipient = recipient
            };

            var financialStatementGeneratorRecipientResultRequest = new RestRequest( "api/FinancialGivingStatement/GetStatementGeneratorRecipientResult", Method.POST );
            financialStatementGeneratorRecipientResultRequest.AddJsonBody( financialStatementGeneratorRecipientRequest );

            Stopwatch getStatementHtml = Stopwatch.StartNew();

            var financialStatementGeneratorRecipientResultResponse = restClient.Execute<Client.FinancialStatementGeneratorRecipientResult>( financialStatementGeneratorRecipientResultRequest );
            if ( financialStatementGeneratorRecipientResultResponse.ErrorException != null )
            {
                throw financialStatementGeneratorRecipientResultResponse.ErrorException;
            }

            Debug.WriteLine( $"{getStatementHtml.Elapsed.TotalMilliseconds}ms, GetStatementGeneratorRecipientResult" );

            FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = financialStatementGeneratorRecipientResultResponse.Data;
            return financialStatementGeneratorRecipientResult;
        }

        private static string GetRockStatementGeneratoryTemporaryDirectory( RockConfig rockConfig )
        {
            var reportTemporaryDirectory = rockConfig.TemporaryDirectory;
            if ( reportTemporaryDirectory.IsNotNullOrWhitespace() )
            {
                Directory.CreateDirectory( reportTemporaryDirectory );
            }
            else
            {
                reportTemporaryDirectory = Path.GetTempPath();
            }

            var reportRockStatementGeneratorTemporaryDirectory = Path.Combine( reportTemporaryDirectory, $"Rock Statement Generator-{DateTime.Now.ToString( "MM_dd_yyyy" )}" );
            Directory.CreateDirectory( reportRockStatementGeneratorTemporaryDirectory );
            return reportRockStatementGeneratorTemporaryDirectory;
        }

        private List<Client.FinancialStatementGeneratorRecipient> GetRecipients( RestClient restClient )
        {


            UpdateProgress( "Getting Recipients..." );

            var financialStatementGeneratorRecipientsRequest = new RestRequest( "api/FinancialGivingStatement/GetFinancialStatementGeneratorRecipients", Method.POST );
            financialStatementGeneratorRecipientsRequest.AddJsonBody( this.Options );
            var financialStatementGeneratorRecipientsResponse = restClient.Execute<List<Client.FinancialStatementGeneratorRecipient>>( financialStatementGeneratorRecipientsRequest );
            if ( financialStatementGeneratorRecipientsResponse.ErrorException != null )
            {
                throw financialStatementGeneratorRecipientsResponse.ErrorException;
            }

            return financialStatementGeneratorRecipientsResponse.Data;

        }

        /// <summary>
        /// Gets the PDF print options.
        /// </summary>
        /// <param name="pdfObjectSettings">The PDF object settings.</param>
        /// <param name="statementGeneratorRecipientResult">The statement generator recipient result.</param>
        /// <returns></returns>
        private static PdfPrintOptions GetPdfPrintOptions( Dictionary<string, string> pdfObjectSettings, FinancialStatementGeneratorRecipientResult statementGeneratorRecipientResult )
        {
            string value;
            PdfPrintOptions pdfPrintOptions = new PdfPrintOptions();

            if ( pdfObjectSettings.TryGetValue( "margin.left", out value ) )
            {
                pdfPrintOptions.MarginLeft = value.AsDouble();
            }

            if ( pdfObjectSettings.TryGetValue( "margin.top", out value ) )
            {
                pdfPrintOptions.MarginTop = value.AsDouble();
            }

            if ( pdfObjectSettings.TryGetValue( "margin.right", out value ) )
            {
                pdfPrintOptions.MarginRight = value.AsDouble();
            }

            if ( pdfObjectSettings.TryGetValue( "margin.bottom", out value ) )
            {
                pdfPrintOptions.MarginBottom = value.AsDouble();
            }

            if ( pdfObjectSettings.TryGetValue( "footer.fontSize", out value ) )
            {
                pdfPrintOptions.Footer.FontSize = value.AsIntegerOrNull() ?? 10;
            }
            else
            {
                pdfPrintOptions.Footer.FontSize = 10;
            }

            var footerHtml = statementGeneratorRecipientResult.FooterHtml;

            if ( footerHtml != null )
            {
                // see https://ironpdf.com/examples/html-headers-and-footers/
                pdfPrintOptions.Footer = new HtmlHeaderFooter()
                {
                    //Height = 15,
                    HtmlFragment = footerHtml,
                    //DrawDividerLine = true
                };
            }

            return pdfPrintOptions;
        }

        /// <summary>
        /// Writes the group of statements to document.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="statementGeneratorRecipientPdfResults">The statement generator recipient PDF results.</param>
        /// <returns></returns>
        private void WriteStatementPDFs( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<StatementGeneratorRecipientPdfResult> statementGeneratorRecipientPdfResults )
        {
            var recipientList = statementGeneratorRecipientPdfResults;

            // use C# to sort the recipients by specified PrimarySortOrder and SecondarySortOrder
            if ( financialStatementReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.LastName )
            {
                var sortedRecipientList = recipientList.OrderBy( a => a.LastName ).ThenBy( a => a.NickName );
                if ( financialStatementReportConfiguration.SecondarySortOrder == FinancialStatementOrderBy.PostalCode )
                {
                    sortedRecipientList = sortedRecipientList.ThenBy( a => a.PostalCode );
                }

                recipientList = sortedRecipientList.ToList();
            }
            else if ( financialStatementReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.PostalCode )
            {
                var sortedRecipientList = recipientList.OrderBy( a => a.PostalCode );
                if ( financialStatementReportConfiguration.SecondarySortOrder == FinancialStatementOrderBy.LastName )
                {
                    sortedRecipientList = sortedRecipientList.ThenBy( a => a.LastName ).ThenBy( a => a.NickName );
                }

                recipientList = sortedRecipientList.ToList();
            }


            var maxStatementsPerChapter = financialStatementReportConfiguration.MaxStatementsPerChapter;
            var fileNamePrefix = financialStatementReportConfiguration.FilenamePrefix;
            var useChapters = financialStatementReportConfiguration.MaxStatementsPerChapter.HasValue;
            var saveDirectory = financialStatementReportConfiguration.DestinationFolder;
            var baseFileName = $"_baseFileName_{DateTime.Now.Ticks}";
            var chapterIndex = 1;

            List<IronPdf.PdfDocument> pdfDocumentList = new List<IronPdf.PdfDocument>();
            foreach ( var result in statementGeneratorRecipientPdfResults )
            {
                pdfDocumentList.Add( PdfDocument.FromFile( result.PdfTempFileName ) );
                Interlocked.Increment( ref _recordsCompleted );
                UpdateProgress( "Loading PDFs" );
            }

            foreach ( var tempFile in statementGeneratorRecipientPdfResults.Select( a => a.PdfTempFileName ) )
            {
                if ( File.Exists( tempFile ) )
                {
                    File.Delete( tempFile );
                }
            }

            if ( pdfDocumentList.Any() )
            {
                var lastPdfDocument = pdfDocumentList.LastOrDefault();
                List<IronPdf.PdfDocument> chapterDocs = new List<IronPdf.PdfDocument>();
                foreach ( var pdfDocument in pdfDocumentList )
                {
                    UpdateProgress( "Creating PDF..." );

                    chapterDocs.Add( pdfDocument );

                    if ( useChapters && ( ( chapterDocs.Count() >= maxStatementsPerChapter ) || pdfDocument == lastPdfDocument ) )
                    {
                        var chapterDoc = IronPdf.PdfDocument.Merge( chapterDocs );
                        var filePath = GetFileName( financialStatementReportConfiguration.DestinationFolder, fileNamePrefix, baseFileName, chapterIndex );
                        SavePdfFile( chapterDoc, filePath );
                        chapterDocs.Clear();
                        chapterIndex++;
                    }
                }

                if ( useChapters )
                {
                    // just in case we still have statements that haven't been written to a pdf
                    if ( chapterDocs.Any() )
                    {
                        var filePath = GetFileName( saveDirectory, fileNamePrefix, baseFileName, chapterIndex );
                        var chapterDoc = IronPdf.PdfDocument.Merge( chapterDocs );
                        SavePdfFile( chapterDoc, filePath );
                    }
                }
                else
                {
                    var chapterDoc = IronPdf.PdfDocument.Merge( chapterDocs );
                    var filePath = GetFileName( saveDirectory, fileNamePrefix, baseFileName, null );
                    SavePdfFile( chapterDoc, filePath );
                }
            }
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="baseName">Name of the base.</param>
        /// <param name="chapterIndex">Index of the chapter.</param>
        /// <returns></returns>
        private string GetFileName( string path, string prefix, string baseName, int? chapterIndex )
        {
            var useChapters = chapterIndex.HasValue;

            if ( !useChapters )
            {
                return $@"{path}\{prefix}{baseName}.pdf";
            }

            return $@"{path}\{prefix}{baseName}-chapter{chapterIndex.Value}.pdf";
        }

        /// <summary>
        /// Saves the PDF file and Prompts if the file seems to be open
        /// </summary>
        /// <param name="resultPdf">The result PDF.</param>
        /// <param name="filePath">The file path.</param>
        private static void SavePdfFile( PdfDocument resultPdf, string filePath )
        {
            if ( File.Exists( filePath ) )
            {
                try
                {
                    File.Delete( filePath );
                }
                catch ( Exception )
                {
                    System.Windows.MessageBox.Show( "Unable to write save PDF File. Make sure you don't have the file open then press OK to try again.", "Warning", System.Windows.MessageBoxButton.OK );
                }
            }

            resultPdf.SaveAs( filePath );
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progressMessage">The message.</param>
        private void UpdateProgress( string progressMessage )
        {
            var position = Interlocked.Read( ref _recordsCompleted );
            OnProgress?.Invoke( this, new ProgressEventArgs { ProgressMessage = progressMessage, Position = ( int ) position, Max = RecordCount } );
        }

        /// <summary>
        /// Occurs when [configuration progress].
        /// </summary>
        public event EventHandler<ProgressEventArgs> OnProgress;
    }

    /// <summary>
    ///
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public int Max { get; set; }

        /// <summary>
        /// Gets or sets the progress message.
        /// </summary>
        /// <value>
        /// The progress message.
        /// </value>
        public string ProgressMessage { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal class StatementGeneratorRecipientPdfResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatementGeneratorRecipientPdfResult"/> class.
        /// </summary>
        /// <param name="statementGeneratorRecipientResult">The statement generator recipient result.</param>
        public StatementGeneratorRecipientPdfResult( FinancialStatementGeneratorRecipientResult statementGeneratorRecipientResult )
        {
            StatementGeneratorRecipientResult = statementGeneratorRecipientResult;
        }

        /// <summary>
        /// Gets the statement generator recipient result.
        /// </summary>
        /// <value>
        /// The statement generator recipient result.
        /// </value>
        public FinancialStatementGeneratorRecipientResult StatementGeneratorRecipientResult { get; private set; }

        /// <summary>
        /// Gets or sets the name of the PDF temporary file.
        /// </summary>
        /// <value>
        /// The name of the PDF temporary file.
        /// </value>
        public string PdfTempFileName { get; set; }

        /// <summary>
        /// The last name of the primary giver for the statement. This will be used in creating merged reports.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName => StatementGeneratorRecipientResult.LastName;

        /// <summary>
        /// Gets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName => StatementGeneratorRecipientResult.NickName;

        /// <summary>
        /// The ZipCode/PostalCode for the address on the statement. This will be used in creating merged reports.
        /// </summary>
        /// <value>
        /// The zip code.
        /// </value>
        public string PostalCode => StatementGeneratorRecipientResult.PostalCode;

        /// <summary>
        /// The total amount of contributions reported on the statement.
        /// </summary>
        /// <value>
        /// The contribution total.
        /// </value>
        public decimal ContributionTotal => StatementGeneratorRecipientResult.ContributionTotal;

        /// <summary>
        /// The total amount of pledges reported on the statement.
        /// </summary>
        /// <value>
        /// The pledge total.
        /// </value>
        public decimal? PledgeTotal => StatementGeneratorRecipientResult.PledgeTotal;

        /// <summary>
        /// The country (if any) for the address on the statement.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public string Country => StatementGeneratorRecipientResult.Country;
    }
}
