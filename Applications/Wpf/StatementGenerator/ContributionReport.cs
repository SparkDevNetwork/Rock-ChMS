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
using System.Windows.Input;

using IronPdf;
using IronPdf.Threading;

using Newtonsoft.Json;

using RestSharp;

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

        private bool _cancelRunning = false;
        private bool _cancelled = false;

        public void Cancel()
        {
            _cancelRunning = true;
        }

        public bool IsCancelled => _cancelled;

        private static long _recordsCompleted = 0;

        private static Task _lastRenderPDFFromHtmlTask = null;

        private static ConcurrentBag<Task> _tasks;

        private static FinancialStatementTemplateReportSettings _reportSettings;

        private static ConcurrentBag<double> _generatePdfTimingsMS = null;
        private static ConcurrentBag<double> _saveAndUploadPdfTimingsMS = null;
        private static ConcurrentBag<double> _waitForLastTaskTimingsMS = null;
        private static ConcurrentBag<double> _getStatementHtmlTimingsMS = null;
        private static ConcurrentBag<StatementGeneratorRecipientPdfResult> _statementGeneratorRecipientPdfResults = null;

        private string ReportRockStatementGeneratorTemporaryDirectory { get; set; }
        private string ReportRockStatementGeneratorStatementsTemporaryDirectory { get; set; }

        /// <summary>
        /// Runs the report returning the number of statements that were generated
        /// </summary>
        /// <returns></returns>
        public int RunReport()
        {
            var licenseKey = File.ReadAllText( "license.key" );
            IronPdf.License.LicenseKey = licenseKey;
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            UpdateProgress( "Connecting..." );

            // Login and setup options for REST calls
            RockConfig rockConfig = RockConfig.Load();

            var restClient = new RestClient( rockConfig.RockBaseUrl );
            restClient.LoginToRock( rockConfig.Username, rockConfig.Password );

            // Get Selected FinancialStatementTemplate
            var getFinancialStatementTemplatesRequest = new RestRequest( $"api/FinancialStatementTemplates/{this.Options.FinancialStatementTemplateId ?? 0}" );
            var getFinancialStatementTemplatesResponse = restClient.Execute<Client.FinancialStatementTemplate>( getFinancialStatementTemplatesRequest );
            if ( getFinancialStatementTemplatesResponse.ErrorException != null )
            {
                throw getFinancialStatementTemplatesResponse.ErrorException;
            }

            Rock.Client.FinancialStatementTemplate financialStatementTemplate = getFinancialStatementTemplatesResponse.Data;

            _reportSettings = JsonConvert.DeserializeObject<Rock.Client.FinancialStatementTemplateReportSettings>( financialStatementTemplate.ReportSettingsJson );

            // Get Recipients from Rock REST Endpoint
            List<FinancialStatementGeneratorRecipient> recipientList = GetRecipients( restClient );
            ReportRockStatementGeneratorTemporaryDirectory = GetRockStatementGeneratorTemporaryDirectory( rockConfig );

            this.RecordCount = recipientList.Count;
            _recordsCompleted = 0;

            _tasks = new ConcurrentBag<Task>();

            _generatePdfTimingsMS = new ConcurrentBag<double>();
            _saveAndUploadPdfTimingsMS = new ConcurrentBag<double>();
            _waitForLastTaskTimingsMS = new ConcurrentBag<double>();
            _getStatementHtmlTimingsMS = new ConcurrentBag<double>();

            _statementGeneratorRecipientPdfResults = new ConcurrentBag<StatementGeneratorRecipientPdfResult>();

            IronPdf.Installation.TempFolderPath = Path.Combine( ReportRockStatementGeneratorTemporaryDirectory, "IronPdf" );
            Directory.CreateDirectory( IronPdf.Installation.TempFolderPath );

            ReportRockStatementGeneratorStatementsTemporaryDirectory = Path.Combine( ReportRockStatementGeneratorTemporaryDirectory, "Statements" );
            Directory.CreateDirectory( ReportRockStatementGeneratorStatementsTemporaryDirectory );

            _lastRenderPDFFromHtmlTask = null;
            UpdateProgress( "Getting Statements..." );
            foreach ( var recipient in recipientList )
            {
                if ( _cancelRunning == true )
                {
                    break;
                }
                StartGenerateStatementForRecipient( recipient, restClient );
                SaveRecipientListStatus( recipientList );
            }

            // some of the 'Save and Upload' tasks could be running, so wait for those
            Task.WaitAll( _tasks.ToArray() );

            if ( _cancelRunning )
            {
                this._cancelled = true;
                return ( int ) _recordsCompleted;
            }

            SaveRecipientListStatus( recipientList );

            UpdateProgress( "Creating PDF..." );

            _statementGeneratorRecipientPdfResults = new ConcurrentBag<StatementGeneratorRecipientPdfResult>( _statementGeneratorRecipientPdfResults.Where( a => a.PdfTempFileName != null ) );
            this.RecordCount = _statementGeneratorRecipientPdfResults.Count();

            _recordsCompleted = 0;


            foreach ( var financialStatementReportConfiguration in this.Options.ReportConfigurationList )
            {
                WriteStatementPDFs( financialStatementReportConfiguration, _statementGeneratorRecipientPdfResults );
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
        /// Starts the generate statement for recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="restClient">The rest client.</param>
        private void StartGenerateStatementForRecipient( FinancialStatementGeneratorRecipient recipient, RestClient restClient )
        {
            FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = GetFinancialStatementGeneratorRecipientResult( restClient, recipient );

            var statementGeneratorPdfResult = new StatementGeneratorRecipientPdfResult( financialStatementGeneratorRecipientResult );
            _statementGeneratorRecipientPdfResults.Add( statementGeneratorPdfResult );

            if ( string.IsNullOrWhiteSpace( financialStatementGeneratorRecipientResult.Html ) )
            {
                // don't generate a statement if no statement HTML
                statementGeneratorPdfResult.PdfTempFileName = null;
                return;
            }

            Stopwatch waitForLastTask = Stopwatch.StartNew();

            // We were able to fetch the HTML for the next statement, and save/upload docs while waiting for the PDF task to finish,
            // but it'll lock up if we don't wait for last PDF generation to complete
            _lastRenderPDFFromHtmlTask?.Wait();
            waitForLastTask.Stop();
            _waitForLastTaskTimingsMS.Add( waitForLastTask.Elapsed.TotalMilliseconds );

            // DEBUG. Which of these methods is faster?
            bool useUniversalRenderJob = true;

            _lastRenderPDFFromHtmlTask = Task.Run( () =>
            {
                var pdfOptions = GetPdfPrintOptions( _reportSettings.PDFObjectSettings, financialStatementGeneratorRecipientResult );
                statementGeneratorPdfResult.PdfTempFileName = Path.Combine( ReportRockStatementGeneratorStatementsTemporaryDirectory, $"GroupId_{financialStatementGeneratorRecipientResult.GroupId}_PersonID_{financialStatementGeneratorRecipientResult.PersonId}.pdf" );

                Stopwatch generatePdfStopWatch = Stopwatch.StartNew();

                IronPdf.PdfDocument pdfDocument;

                if ( useUniversalRenderJob )
                {
                    UniversalRenderJob universalRenderJob = new UniversalRenderJob();
                    universalRenderJob.Options = pdfOptions;
                    universalRenderJob.Html = statementGeneratorPdfResult.StatementGeneratorRecipientResult.Html;

                    // this is not thread-safe, so we'll wait to do this until the last one is done
                    // In the mean time, we'll
                    //   -- Start a thread that saves the previously completed document to the temp directory and upload it (if configured to do so).
                    //   -- Get the HTML for the next recipient
                    var pdfBytes = universalRenderJob.DoRemoteRender();
                    pdfDocument = new PdfDocument( pdfBytes );
                }
                else
                {
                    // this is not thread-safe, even with IronPdf.Threading installed, so we'll wait to do this until the last one is done
                    // In the mean time, we'll
                    //   -- Start a thread that saves the previously completed document to the temp directory and upload it (if configured to do so).
                    //   -- Get the HTML for the next recipient
                    pdfDocument = HtmlToPdf.StaticRenderHtmlAsPdf( statementGeneratorPdfResult.StatementGeneratorRecipientResult.Html, pdfOptions );
                }

                generatePdfStopWatch.Stop();

                var recordsCompleted = Interlocked.Increment( ref _recordsCompleted );

                // launch a task to save and upload the document
                // This is thread safe, so we can spin these up as needed 
                _tasks.Add( Task.Run( () =>
                {
                    Stopwatch savePdfStopWatch = Stopwatch.StartNew();
                    recipient.RenderedPageCount = pdfDocument.PageCount;
                    pdfDocument.SaveAs( statementGeneratorPdfResult.PdfTempFileName );

                    pdfDocument.Dispose();

                    // todo Upload Document too

                    recipient.IsComplete = true;
                    if ( recordsCompleted > 2 && Debugger.IsAttached )
                    {
                        _saveAndUploadPdfTimingsMS.Add( savePdfStopWatch.Elapsed.TotalMilliseconds );
                    }
                } ) );

                if ( recordsCompleted > 2 && Debugger.IsAttached && recordsCompleted % 20 == 0 )
                {
                    _generatePdfTimingsMS.Add( generatePdfStopWatch.Elapsed.TotalMilliseconds );

                    var averageGetStatementHtmlTimingsMS = _getStatementHtmlTimingsMS.Any() ? Math.Round( _getStatementHtmlTimingsMS.Average(), 0 ) : 0;
                    var averageWaitForLastTaskTimingsMS = _waitForLastTaskTimingsMS.Any() ? Math.Round( _waitForLastTaskTimingsMS.Average(), 0 ) : 0;
                    var averageGeneratePDFTimingMS = Math.Round( _generatePdfTimingsMS.Average(), 0 );
                    var averageSaveAndUploadPDFTimingMS = _saveAndUploadPdfTimingsMS.Any() ?
                        Math.Round( _saveAndUploadPdfTimingsMS.Average(), 0 )
                        : ( double? ) null;

                    Debug.WriteLine( $@"
Generate     PDF Avg: {averageGeneratePDFTimingMS} ms (useUniversalRenderJob:{useUniversalRenderJob})
GetStatementHtml Avg: {averageGetStatementHtmlTimingsMS} ms)
WaitForLastTask  Avg: {averageWaitForLastTaskTimingsMS} ms)
Save/Upload  PDF Avg: {averageSaveAndUploadPDFTimingMS} ms)

_recordsCompleted:{recordsCompleted}
" );
                }
            } );

            _tasks.Add( _lastRenderPDFFromHtmlTask );

            UpdateProgress( "Processing..." );
        }

        /// <summary>
        private void SaveRecipientListStatus( List<FinancialStatementGeneratorRecipient> recipientList )
        {
            var recipientListJson = Newtonsoft.Json.JsonConvert.SerializeObject( recipientList );
            var recipientListJsonFileName = Path.Combine( ReportRockStatementGeneratorTemporaryDirectory, "RecipientData.Json" );
            File.WriteAllText( recipientListJsonFileName, recipientListJson );
        }

        /// <summary>
        /// Gets the financial statement generator recipient result.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
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

            getStatementHtml.Stop();

            _getStatementHtmlTimingsMS.Add( getStatementHtml.Elapsed.TotalMilliseconds );

            FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = financialStatementGeneratorRecipientResultResponse.Data;
            return financialStatementGeneratorRecipientResult;
        }

        /// <summary>
        /// Gets the rock statement generator temporary directory.
        /// </summary>
        /// <param name="rockConfig">The rock configuration.</param>
        /// <returns></returns>
        private static string GetRockStatementGeneratorTemporaryDirectory( RockConfig rockConfig )
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

        /// <summary>
        /// Gets the recipients.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <returns></returns>
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
            PdfPrintOptions pdfPrintOptions = new IronPdf.PdfPrintOptions();

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

            // see https://ironpdf.com/examples/html-headers-and-footers/
            pdfPrintOptions.Footer.LeftText = statementGeneratorRecipientResult.FooterLeftText;
            pdfPrintOptions.Footer.CenterText = statementGeneratorRecipientResult.FooterCenterText;
            pdfPrintOptions.Footer.RightText = statementGeneratorRecipientResult.FooterRightText;

            if ( pdfObjectSettings.TryGetValue( "footer.fontSize", out value ) )
            {
                pdfPrintOptions.Footer.FontSize = value.AsIntegerOrNull() ?? 10;
            }
            else
            {
                pdfPrintOptions.Footer.FontSize = 10;
            }

            return pdfPrintOptions;
        }

        /// <summary>
        /// Writes the group of statements to document.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="statementGeneratorRecipientPdfResults">The statement generator recipient PDF results.</param>
        /// <returns></returns>
        private void WriteStatementPDFs( FinancialStatementReportConfiguration financialStatementReportConfiguration, ConcurrentBag<StatementGeneratorRecipientPdfResult> statementGeneratorRecipientPdfResults )
        {
            if ( !statementGeneratorRecipientPdfResults.Any() )
            {
                return;
            }

            var recipientList = statementGeneratorRecipientPdfResults.Where( a => a.PdfDocument != null ).ToList();

            if ( financialStatementReportConfiguration.ExcludeOptedOutIndividuals )
            {
                recipientList = recipientList.Where( a => a.StatementGeneratorRecipientResult.OptedOut == false ).ToList();
            }

            if ( financialStatementReportConfiguration.MinimumContributionAmount.HasValue )
            {
                recipientList = recipientList.Where( a => a.StatementGeneratorRecipientResult.ContributionTotal >= financialStatementReportConfiguration.MinimumContributionAmount.Value ).ToList();
            }

            if ( financialStatementReportConfiguration.IncludeInternationalAddresses == false )
            {
                recipientList = recipientList.Where( a => a.IsInternationalAddress == false ).ToList();
            }

            // load documents. We'll need them all loaded in case we needed to sort by PageNumber.
            foreach ( var result in recipientList )
            {
                result.PdfDocument = PdfDocument.FromFile( result.PdfTempFileName );
                Interlocked.Increment( ref _recordsCompleted );
                UpdateProgress( "Loading PDFs" );
            }

            IOrderedEnumerable<StatementGeneratorRecipientPdfResult> sortedRecipientList = SortByPrimaryAndSecondaryOrder( financialStatementReportConfiguration, recipientList );
            recipientList = sortedRecipientList.ToList();

            // remove temp files (including ones from opted out)
            foreach ( var tempFile in statementGeneratorRecipientPdfResults.Select( a => a.PdfTempFileName ) )
            {
                if ( File.Exists( tempFile ) )
                {
                    File.Delete( tempFile );
                }
            }

            var maxStatementsPerChapter = financialStatementReportConfiguration.MaxStatementsPerChapter;

            var useChapters = financialStatementReportConfiguration.MaxStatementsPerChapter.HasValue;

            if ( !useChapters )
            {
                // no chapters, so just write all to one single document
                var allPdfs = recipientList.Select( a => a.PdfDocument ).Where( a => a != null ).ToList();
                if ( !allPdfs.Any() )
                {
                    return;
                }

                var singleFinalDoc = IronPdf.PdfDocument.Merge( allPdfs );
                var singleFileName = Path.Combine( financialStatementReportConfiguration.DestinationFolder, "statements.pdf" );

                singleFinalDoc.PrintToFile( singleFileName );
                return;
            }

            // saving as chapter documents

            // TODO
        }

        /// <summary>
        /// Sorts the by primary and secondary order.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="recipientList">The recipient list.</param>
        /// <returns></returns>
        private static IOrderedEnumerable<StatementGeneratorRecipientPdfResult> SortByPrimaryAndSecondaryOrder( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<StatementGeneratorRecipientPdfResult> recipientList )
        {
            IOrderedEnumerable<StatementGeneratorRecipientPdfResult> sortedRecipientList;

            switch ( financialStatementReportConfiguration.PrimarySortOrder )
            {
                case FinancialStatementOrderBy.PageCount:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.PdfDocument.PageCount );
                        break;
                    }
                case FinancialStatementOrderBy.PostalCode:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.PostalCode );
                        break;
                    }
                case FinancialStatementOrderBy.LastName:
                default:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.LastName ).ThenBy( a => a.NickName );
                        break;
                    }
            }

            switch ( financialStatementReportConfiguration.SecondarySortOrder )
            {
                case FinancialStatementOrderBy.PageCount:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.PdfDocument.PageCount );
                        break;
                    }
                case FinancialStatementOrderBy.PostalCode:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.PostalCode );
                        break;
                    }
                case FinancialStatementOrderBy.LastName:
                default:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.LastName ).ThenBy( a => a.NickName );
                        break;
                    }
            }

            return sortedRecipientList;
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

        /// <inheritdoc cref="FinancialStatementGeneratorRecipientResult.Country"/>
        public string Country => StatementGeneratorRecipientResult.Country;

        /// <summary>
        /// Gets a value indicating whether this instance is international address.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is international address; otherwise, <c>false</c>.
        /// </value>
        public bool IsInternationalAddress => StatementGeneratorRecipientResult.IsInternationalAddress;

        /// <summary>
        /// The PDF document
        /// </summary>
        internal PdfDocument PdfDocument;
    }
}
