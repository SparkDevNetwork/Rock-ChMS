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
using Rock.Net;

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
        /// The _rock rest client
        /// </summary>
        private RestClient _rockRestClient = null;

        /// <summary>
        /// Gets or sets the record count.
        /// </summary>
        /// <value>
        /// The record count.
        /// </value>
        private int RecordCount { get; set; }

        private static long RecordsCompleted = 0;

        /// <summary>
        /// Runs the report returning the number of statements that were generated
        /// </summary>
        /// <returns></returns>
        public int RunReport( FinancialStatementReportConfiguration financialStatementReportConfiguration )
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            UpdateProgress( "Connecting..." );

            // Login and setup options for REST calls
            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RestClient( rockConfig.RockBaseUrl );
            _rockRestClient.CookieContainer = new System.Net.CookieContainer();
            var rockLoginRequest = new RockLoginRequest( rockConfig.Username, rockConfig.Password );
            var rockLoginResponse = _rockRestClient.Execute( rockLoginRequest );

            Rock.Client.FinancialStatementTemplate financialStatementTemplate = new FinancialStatementTemplate();

            var reportSettings = JsonConvert.DeserializeObject<Rock.Client.FinancialStatementTemplateReportSettings>( financialStatementTemplate.ReportSettingsJson );

            var pdfObjectSettings = reportSettings.PDFObjectSettings;
            this.Options.SelectedReportConfiguration = financialStatementReportConfiguration;

            UpdateProgress( "Getting Recipients..." );

            var financialStatementGeneratorRecipientsRequest = new RestRequest( "api/FinancialGivingStatement/GetFinancialStatementGeneratorRecipients", Method.POST );
            financialStatementGeneratorRecipientsRequest.AddJsonBody( this.Options );
            var financialStatementGeneratorRecipientsResponse = _rockRestClient.Execute<List<Client.FinancialStatementGeneratorRecipient>>( financialStatementGeneratorRecipientsRequest );
            List<Client.FinancialStatementGeneratorRecipient> recipientList = financialStatementGeneratorRecipientsResponse.Data;

            this.RecordCount = recipientList.Count;
            RecordsCompleted = 0;

            var tasks = new List<Task>();

            List<StatementGeneratorRecipientPdfResult> statementGeneratorRecipientPdfResults = new List<StatementGeneratorRecipientPdfResult>();

            bool cancel = false;


            Task lastTask = null;

            UpdateProgress( "Getting Statements..." );
            foreach ( var recipient in recipientList )
            {
                FinancialStatementGeneratorRecipientRequest financialStatementGeneratorRecipientRequest = new FinancialStatementGeneratorRecipientRequest()
                {
                    FinancialStatementGeneratorOptions = this.Options,
                    FinancialStatementGeneratorRecipient = recipient
                };

                var financialStatementGeneratorRecipientResultRequest = new RestRequest( "api/FinancialGivingStatement/GetStatementGeneratorRecipientResult", Method.POST );
                financialStatementGeneratorRecipientResultRequest.AddJsonBody( financialStatementGeneratorRecipientRequest );
                var financialStatementGeneratorRecipientResultResponse = _rockRestClient.Execute<Client.FinancialStatementGeneratorRecipientResult>( financialStatementGeneratorRecipientResultRequest );
                FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = financialStatementGeneratorRecipientResultResponse.Data;

                var statementGeneratorPdfResult = new StatementGeneratorRecipientPdfResult( financialStatementGeneratorRecipientResult );
                statementGeneratorRecipientPdfResults.Add( statementGeneratorPdfResult );
                
                if ( ( financialStatementReportConfiguration.ExcludeOptedOutIndividuals && financialStatementGeneratorRecipientResult.OptedOut ) || ( string.IsNullOrWhiteSpace( financialStatementGeneratorRecipientResult.Html ) ) )
                {
                    // don't generate a statement if opted out or no statement html
                    statementGeneratorPdfResult.PdfTempFileName = null;
                }
                else
                {
                    var html = financialStatementGeneratorRecipientResult.Html;
                    var footerHtml = financialStatementGeneratorRecipientResult.FooterHtml;

                    // We were able to fetch the HTML for the next state while waiting for the PDF task to finish,
                    // but it'll lock up if we don't wait for it to complete
                    lastTask?.Wait();

                    lastTask = Task.Run( () =>
                    {
                        Stopwatch generatePdfStopWatch = Stopwatch.StartNew();
                        var pdfOptions = GetPdfPrintOptions( pdfObjectSettings, statementGeneratorPdfResult.StatementGeneratorRecipientResult );
                        bool useUniversalRenderJob = true;

                        if ( useUniversalRenderJob )
                        {
                            UniversalRenderJob universalRenderJob = new UniversalRenderJob();
                            universalRenderJob.Options = pdfOptions;
                            universalRenderJob.Html = statementGeneratorPdfResult.StatementGeneratorRecipientResult.Html;
                            var pdfBytes = universalRenderJob.DoRemoteRender();
                            statementGeneratorPdfResult.PdfTempFileName = Path.GetTempFileName();
                            File.WriteAllBytes( statementGeneratorPdfResult.PdfTempFileName, pdfBytes );
                        }
                        else
                        {
                            var pdfDoc = HtmlToPdf.StaticRenderHtmlAsPdf( statementGeneratorPdfResult.StatementGeneratorRecipientResult.Html, pdfOptions );
                            statementGeneratorPdfResult.PdfTempFileName = Path.GetTempFileName();
                            pdfDoc.SaveAs( statementGeneratorPdfResult.PdfTempFileName );
                        }

                        Interlocked.Increment( ref RecordsCompleted );
                        Debug.WriteLine( $"{generatePdfStopWatch.Elapsed.TotalMilliseconds}ms, generatePdf" );
                        Debug.WriteLine( $"RecordsCompleted:{Interlocked.Read( ref RecordsCompleted ) }\n" );


                    } );

                    tasks.Add( lastTask );

                    tasks = tasks.Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();
                }
                
                UpdateProgress( "Processing..." );
                if ( cancel )
                {
                    break;
                }
            }

            Task.WaitAll( tasks.ToArray() );

            UpdateProgress( "Creating PDF..." );

            statementGeneratorRecipientPdfResults = statementGeneratorRecipientPdfResults.Where( a => a.PdfTempFileName != null ).ToList();
            this.RecordCount = statementGeneratorRecipientPdfResults.Count();
            int? maxStatementsPerChapter = null;

            if ( financialStatementReportConfiguration.MaxStatementsPerChapter.HasValue )
            {
                maxStatementsPerChapter = financialStatementReportConfiguration.MaxStatementsPerChapter.Value;
            }

            if ( maxStatementsPerChapter.HasValue && maxStatementsPerChapter < 1 )
            {
                // just in case they entered 0 or a negative number
                maxStatementsPerChapter = null;
            }

            IEnumerable<IronPdf.PdfDocument> pdfDocumentList = statementGeneratorRecipientPdfResults.Select( a => PdfDocument.FromFile( a.PdfTempFileName ) ).ToList();

            foreach ( var tempFile in statementGeneratorRecipientPdfResults.Select( a => a.PdfTempFileName ) )
            {
                if ( File.Exists( tempFile ) )
                {
                    File.Delete( tempFile );
                }
            }
           

            WriteStatementPDFs( financialStatementReportConfiguration, pdfDocumentList );

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
        /// <param name="fileNamePrefix">The file name prefix.</param>
        /// <param name="pdfDocumentList">The PDF document list.</param>
        /// <param name="maxStatementsPerChapter">The maximum statements per chapter.</param>
        private void WriteStatementPDFs( FinancialStatementReportConfiguration financialStatementReportConfiguration, IEnumerable<IronPdf.PdfDocument> pdfDocumentList )
        {
            var maxStatementsPerChapter = financialStatementReportConfiguration.MaxStatementsPerChapter;
            var fileNamePrefix = financialStatementReportConfiguration.FilenamePrefix;
            var useChapters = financialStatementReportConfiguration.MaxStatementsPerChapter.HasValue;
            var saveDirectory = financialStatementReportConfiguration.DestinationFolder;
            var baseFileName = $"_baseFileName_{DateTime.Now.Ticks}";

            //var statementsInChapter = 0;
            var chapterIndex = 1;

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
            var position = Interlocked.Read( ref RecordsCompleted );
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

    internal class StatementGeneratorRecipientPdfResult
    {
        public StatementGeneratorRecipientPdfResult( FinancialStatementGeneratorRecipientResult statementGeneratorRecipientResult )
        {
            StatementGeneratorRecipientResult = statementGeneratorRecipientResult;
        }



        public FinancialStatementGeneratorRecipientResult StatementGeneratorRecipientResult { get; private set; }
        //public IronPdf.PdfDocument PdfDoc { get; internal set; }
        public string PdfTempFileName { get; set; }
        public int GroupId => StatementGeneratorRecipientResult.GroupId;

        //public byte[] PdfBytes { get; internal set; }
    }
}
