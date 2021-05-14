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
using System.Threading;
using System.Threading.Tasks;

using IronPdf;

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

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        public void Cancel()
        {
            _cancelRunning = true;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is canceled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is canceled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCancelled => _cancelled;

        /// <summary>
        /// Gets the records completed count.
        /// </summary>
        /// <value>
        /// The records completed count.
        /// </value>
        public static long RecordsCompletedCount => Interlocked.Read( ref _recordsCompleted );

        private static long _recordsCompleted = 0;

        private static Task _lastRenderPDFFromHtmlTask = null;

        private static ConcurrentBag<Task> _saveAndUploadTasks;

        private static ConcurrentBag<Task> _tasks;

        private static FinancialStatementTemplateReportSettings _reportSettings;

        private static ConcurrentBag<double> _generatePdfTimingsMS = null;
        private static ConcurrentBag<double> _saveAndUploadPdfTimingsMS = null;
        private static ConcurrentBag<double> _waitForLastTaskTimingsMS = null;
        private static ConcurrentBag<double> _getStatementHtmlTimingsMS = null;
        private static ConcurrentBag<FinancialStatementGeneratorRecipientResult> _financialStatementGeneratorRecipientResults = null;

        /// <summary>
        /// Gets a value indicating whether this <see cref="ContributionReport"/> is resume.
        /// </summary>
        /// <value>
        ///   <c>true</c> if resume; otherwise, <c>false</c>.
        /// </value>
        public static bool Resume { get; internal set; } = false;

        private static FinancialStatementIndividualSaveOptions _individualSaveOptions;
        private static bool _saveStatementsForIndividualsToDocument;

        private static RestClient _uploadPdfDocumentRestClient;

        /// <summary>
        /// The start date time
        /// </summary>
        public static DateTime StartDateTime { get; private set; }

        private static Stopwatch _stopwatchAll;
        private static Stopwatch _stopwatchRenderPDFsOverall;

        private string _currentDayTemporaryDirectory { get; set; }

        /// <summary>
        /// Runs the report returning the number of statements that were generated
        /// </summary>
        /// <returns></returns>
        public int RunReport()
        {
            StartDateTime = DateTime.Now;
            var licenseKey = File.ReadAllText( "license.key" );
            IronPdf.License.LicenseKey = licenseKey;
            _stopwatchAll = new Stopwatch();
            _stopwatchAll.Start();

            RockConfig rockConfig = RockConfig.Load();

            UpdateProgress( "Connecting...", 0, 0 );

            // Login and setup options for REST calls
            var restClient = new RestClient( rockConfig.RockBaseUrl );
            restClient.LoginToRock( rockConfig.Username, rockConfig.Password );

            _individualSaveOptions = rockConfig.IndividualSaveOptionsJson.FromJsonOrNull<FinancialStatementIndividualSaveOptions>();
            _saveStatementsForIndividualsToDocument = _individualSaveOptions.SaveStatementsForIndividuals;
            if ( _saveStatementsForIndividualsToDocument )
            {
                _uploadPdfDocumentRestClient = new RestClient( rockConfig.RockBaseUrl );
                _uploadPdfDocumentRestClient.LoginToRock( rockConfig.Username, rockConfig.Password );
            }
            else
            {
                _uploadPdfDocumentRestClient = null;
            }

            FinancialStatementTemplate financialStatementTemplate = GetFinancialStatementTemplate( rockConfig, restClient );

            _reportSettings = financialStatementTemplate.ReportSettingsJson.FromJsonOrNull<FinancialStatementTemplateReportSettings>();

            List<FinancialStatementGeneratorRecipient> recipientList;

            if ( Resume )
            {
                // Get Recipients from Rock REST Endpoint from incomplete session
                UpdateProgress( "Resuming Incomplete Recipients...", 0, 0 );
                recipientList = GetSavedRecipientList();

                var savedRecipientsResults = GetSavedRecipientResults();
                if ( savedRecipientsResults != null )
                {
                    _financialStatementGeneratorRecipientResults = new ConcurrentBag<FinancialStatementGeneratorRecipientResult>( savedRecipientsResults );
                }
                else
                {
                    // this could happen if we are resuming and but nobody in the list is completed
                    _financialStatementGeneratorRecipientResults = new ConcurrentBag<FinancialStatementGeneratorRecipientResult>();
                }
            }
            else
            {
                // Get Recipients from Rock REST Endpoint
                UpdateProgress( "Getting Statement Recipients...", 0, 0 );
                recipientList = GetRecipients( restClient );
            }

            _currentDayTemporaryDirectory = GetStatementGeneratorTemporaryDirectory( rockConfig, DateTime.Today );

            this.RecordCount = recipientList.Count;
            _recordsCompleted = 0;

            _tasks = new ConcurrentBag<Task>();
            _saveAndUploadTasks = new ConcurrentBag<Task>();

            _generatePdfTimingsMS = new ConcurrentBag<double>();
            _saveAndUploadPdfTimingsMS = new ConcurrentBag<double>();
            _waitForLastTaskTimingsMS = new ConcurrentBag<double>();
            _getStatementHtmlTimingsMS = new ConcurrentBag<double>();

            IronPdf.Installation.TempFolderPath = Path.Combine( _currentDayTemporaryDirectory, "IronPdf" );

            Directory.CreateDirectory( IronPdf.Installation.TempFolderPath );
            Directory.CreateDirectory( Path.Combine( _currentDayTemporaryDirectory, "Statements" ) );

            _lastRenderPDFFromHtmlTask = null;

            var recipientProgressMax = recipientList.Count;

            StartDateTime = DateTime.Now;
            _stopwatchRenderPDFsOverall = Stopwatch.StartNew();
            List<FinancialStatementGeneratorRecipient> incompleteRecipients;
            int progressOffset = 0;
            if ( Resume )
            {
                incompleteRecipients = recipientList.Where( a => !a.IsComplete ).ToList();
                progressOffset = recipientList.Where( a => a.IsComplete ).Count();
            }
            else
            {
                incompleteRecipients = recipientList;
            }

            foreach ( var recipient in incompleteRecipients )
            {
                if ( _cancelRunning == true )
                {
                    break;
                }

                var recipientProgressPosition = Interlocked.Read( ref _recordsCompleted );

                UpdateProgress( "Generating Individual Documents...", recipientProgressPosition + progressOffset, recipientProgressMax );
                if ( Resume && recipient.IsComplete )
                {
                    Interlocked.Increment( ref _recordsCompleted );
                    continue;
                }

                StartGenerateStatementForRecipient( recipient, restClient );
                SaveRecipientListStatus( recipientList, _currentDayTemporaryDirectory );
            }

            if ( this.Options.EnablePageCountPredetermination )
            {
                // all the render tasks should be done, but just in case
                UpdateProgress( $"Finishing up tasks (first pass)", 0, 0 );
                Task.WaitAll( _tasks.ToArray() );

                _recordsCompleted = 0;
                foreach ( var recipient in recipientList )
                {
                    if ( _cancelRunning == true )
                    {
                        break;
                    }

                    var recipientProgressPosition = Interlocked.Read( ref _recordsCompleted );
                    var secondPassPosition = recipientProgressPosition / 2;

                    UpdateProgress( "Generating Individual Documents (2nd Pass)...", recipientProgressPosition, recipientProgressMax );

                    StartGenerateStatementForRecipient( recipient, restClient );
                    SaveRecipientListStatus( recipientList, _currentDayTemporaryDirectory );
                }
            }

            _lastRenderPDFFromHtmlTask?.Wait();

            // all the render tasks should be done, but just in case
            UpdateProgress( $"Finishing up tasks", 0, 0 );

            // some of the 'Save and Upload' tasks could be running, so wait for those
            var remainingRenderTasks = _tasks.ToArray().Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();
            while ( remainingRenderTasks.Any() )
            {
                var finishedTask = Task.WhenAny( remainingRenderTasks.ToArray() );
                remainingRenderTasks = remainingRenderTasks.ToArray().Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();
                var recipientProgressPosition = Interlocked.Read( ref _recordsCompleted );
                UpdateProgress( $"Finishing up {remainingRenderTasks.Count() } Individual Statements...", recipientProgressPosition, recipientProgressMax );
            }

            Task.WaitAll( _tasks.ToArray() );

            // some of the 'Save and Upload' tasks could be running, so wait for those
            var remainingDocumentUploadTasks = _saveAndUploadTasks.ToArray().Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();
            while ( remainingDocumentUploadTasks.Any() )
            {
                var finishedTask = Task.WhenAny( remainingDocumentUploadTasks.ToArray() );
                remainingDocumentUploadTasks = remainingDocumentUploadTasks.ToArray().Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();
                UpdateProgress( $"Finishing up {remainingDocumentUploadTasks.Count() } document uploads...", 0, 0 );
            }

            Task.WaitAll( remainingDocumentUploadTasks.ToArray() );

            if ( _cancelRunning )
            {
                this._cancelled = true;
                return ( int ) _recordsCompleted;
            }

            SaveRecipientListStatus( recipientList, _currentDayTemporaryDirectory );

            _financialStatementGeneratorRecipientResults = new ConcurrentBag<FinancialStatementGeneratorRecipientResult>( _financialStatementGeneratorRecipientResults.Where( a => a.Html != null ) );
            this.RecordCount = recipientList.Count( x => x.IsComplete );

            var reportCount = this.Options.ReportConfigurationList.Count();
            var reportNumber = 0;

            foreach ( var financialStatementReportConfiguration in this.Options.ReportConfigurationList )
            {
                reportNumber++;
                if ( reportCount == 1 )
                {
                    UpdateProgress( "Generating Report...", 0, 0 );
                }
                else
                {
                    UpdateProgress( $"Generating Report {reportNumber}", reportNumber, reportCount );
                }

                WriteStatementPDFs( financialStatementReportConfiguration, _financialStatementGeneratorRecipientResults );
            }

            UpdateProgress( "Cleaning up temporary files.", 0, 0 );

            // remove temp files (including ones from opted out)
            foreach ( var pdfTempFilePath in recipientList.Select( a => a.GetPdfDocumentFilePath( _currentDayTemporaryDirectory ) ) )
            {
                if ( File.Exists( pdfTempFilePath ) )
                {
                    File.Delete( pdfTempFilePath );
                }
            }

            UpdateProgress( "Complete", 0, 0 );

            _stopwatchAll.Stop();
            var elapsedSeconds = _stopwatchAll.ElapsedMilliseconds / 1000;
            Debug.WriteLine( $"{elapsedSeconds:n0} seconds" );
            Debug.WriteLine( $"{RecordCount:n0} statements" );
            if ( RecordCount > 0 )
            {
                Debug.WriteLine( $"{( _stopwatchAll.ElapsedMilliseconds / RecordCount ):n0}ms per statement" );
            }

            return this.RecordCount;
        }

        /// <summary>
        /// Gets the financial statement template.
        /// </summary>
        /// <param name="rockConfig">The rock configuration.</param>
        /// <param name="restClient">The rest client.</param>
        /// <returns></returns>
        private FinancialStatementTemplate GetFinancialStatementTemplate( RockConfig rockConfig, RestClient restClient )
        {
            if ( !this.Options.FinancialStatementTemplateId.HasValue )
            {
                var getFinancialStatementTemplateIdRequest = new RestRequest( $"api/FinancialStatementTemplates?$filter=Guid eq guid'{rockConfig.FinancialStatementTemplateGuid}'" );
                this.Options.FinancialStatementTemplateId = restClient.Execute<List<FinancialStatementTemplate>>( getFinancialStatementTemplateIdRequest ).Data.FirstOrDefault()?.Id;
            }

            var getFinancialStatementTemplatesRequest = new RestRequest( $"api/FinancialStatementTemplates/{this.Options.FinancialStatementTemplateId ?? 0}" );
            var getFinancialStatementTemplatesResponse = restClient.Execute<Client.FinancialStatementTemplate>( getFinancialStatementTemplatesRequest );

            if ( getFinancialStatementTemplatesResponse.ErrorException != null )
            {
                throw getFinancialStatementTemplatesResponse.ErrorException;
            }

            Rock.Client.FinancialStatementTemplate financialStatementTemplate = getFinancialStatementTemplatesResponse.Data;
            if ( !this.Options.FinancialStatementTemplateId.HasValue )
            {
                this.Options.FinancialStatementTemplateId = financialStatementTemplate.Id;
            }

            return financialStatementTemplate;
        }

        /// <summary>
        /// Starts the generate statement for recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="restClient">The rest client.</param>
        private void StartGenerateStatementForRecipient( FinancialStatementGeneratorRecipient recipient, RestClient restClient )
        {
            FinancialStatementGeneratorRecipientResult financialStatementGeneratorRecipientResult = GetFinancialStatementGeneratorRecipientResult( restClient, recipient );

            var statementGeneratorPdfResult = financialStatementGeneratorRecipientResult;
            _financialStatementGeneratorRecipientResults.Add( statementGeneratorPdfResult );
            SaveRecipientResults( _financialStatementGeneratorRecipientResults, _currentDayTemporaryDirectory );

            if ( string.IsNullOrWhiteSpace( financialStatementGeneratorRecipientResult.Html ) )
            {
                // don't generate a statement if no statement HTML
                return;
            }

            Stopwatch waitForLastTask = Stopwatch.StartNew();

            waitForLastTask.Stop();
            _waitForLastTaskTimingsMS.Add( waitForLastTask.Elapsed.TotalMilliseconds );

            _lastRenderPDFFromHtmlTask = new Task( () =>
            {
                var pdfOptions = GetPdfPrintOptions( _reportSettings.PDFSettings, financialStatementGeneratorRecipientResult );

                Stopwatch generatePdfStopWatch = Stopwatch.StartNew();

                IronPdf.PdfDocument pdfDocument = pdfDocument = HtmlToPdf.StaticRenderHtmlAsPdf( financialStatementGeneratorRecipientResult.Html, pdfOptions );

                generatePdfStopWatch.Stop();

                var recordsCompleted = Interlocked.Increment( ref _recordsCompleted );

                // launch a task to save and upload the document
                // This is thread safe, so we can spin these up as needed 
                var saveAndUploadTask = new Task( () =>
               {
                   Stopwatch savePdfStopWatch = Stopwatch.StartNew();
                   recipient.RenderedPageCount = pdfDocument.PageCount;

                   var pdfTempFilePath = statementGeneratorPdfResult.Recipient.GetPdfDocumentFilePath( _currentDayTemporaryDirectory );

                   pdfDocument.SaveAs( pdfTempFilePath );

                   if ( _saveStatementsForIndividualsToDocument )
                   {
                       FinancialStatementGeneratorUploadGivingStatementData uploadGivingStatementData = new FinancialStatementGeneratorUploadGivingStatementData
                       {
                           FinancialStatementGeneratorRecipient = recipient,
                           FinancialStatementIndividualSaveOptions = _individualSaveOptions,
                           PDFData = pdfDocument.BinaryData
                       };

                       RestRequest uploadDocumentRequest = new RestRequest( "api/FinancialGivingStatement/UploadGivingStatementDocument" );
                       uploadDocumentRequest.AddJsonBody( uploadGivingStatementData );

                       var uploadDocumentResponse = _uploadPdfDocumentRestClient.ExecutePostAsync( uploadDocumentRequest ).Result;
                       if ( uploadDocumentResponse.ErrorException != null )
                       {
                           throw uploadDocumentResponse.ErrorException;
                       }
                   }

                   recipient.IsComplete = true;
                   if ( recordsCompleted > 2 && Debugger.IsAttached )
                   {
                       _saveAndUploadPdfTimingsMS.Add( savePdfStopWatch.Elapsed.TotalMilliseconds );
                   }
               } );

                _saveAndUploadTasks.Add( saveAndUploadTask );
                saveAndUploadTask.Start();

                if ( recordsCompleted > 2 && Debugger.IsAttached && recordsCompleted % 10 == 0 )
                {
                    _generatePdfTimingsMS.Add( generatePdfStopWatch.Elapsed.TotalMilliseconds );

                    var averageGetStatementHtmlTimingsMS = _getStatementHtmlTimingsMS.Any() ? Math.Round( _getStatementHtmlTimingsMS.Average(), 0 ) : 0;
                    var averageWaitForLastTaskTimingsMS = _waitForLastTaskTimingsMS.Any() ? Math.Round( _waitForLastTaskTimingsMS.Average(), 0 ) : 0;
                    var averageGeneratePDFTimingMS = Math.Round( _generatePdfTimingsMS.Average(), 0 );
                    var averageSaveAndUploadPDFTimingMS = _saveAndUploadPdfTimingsMS.Any() ?
                        Math.Round( _saveAndUploadPdfTimingsMS.Average(), 0 )
                        : ( double? ) null;

                    Debug.WriteLine( $@"
GeneratePDF/thread Avg: {averageGeneratePDFTimingMS} ms)
GetStatementHtml   Avg: {averageGetStatementHtmlTimingsMS} ms)
WaitForLastTask    Avg: {averageWaitForLastTaskTimingsMS} ms)
Save/Upload  PDF   Avg: {averageSaveAndUploadPDFTimingMS} ms)
Total PDFs Elapsed    : {_stopwatchRenderPDFsOverall.Elapsed.TotalMilliseconds} ms 
_recordsCompleted     : {recordsCompleted}
Overall ms/PDF     Avg: {_stopwatchRenderPDFsOverall.Elapsed.TotalMilliseconds / recordsCompleted} ms
Overall PDF/sec    Avg: {recordsCompleted / _stopwatchRenderPDFsOverall.Elapsed.TotalSeconds }/sec
" );
                }
            } );

            _tasks.Add( _lastRenderPDFFromHtmlTask );

            _lastRenderPDFFromHtmlTask.Start();
        }

        /// <summary>
        private static void SaveRecipientListStatus( List<FinancialStatementGeneratorRecipient> recipientList, string reportRockStatementGeneratorTemporaryDirectory )
        {
            var recipientListJson = recipientList.ToJson( Formatting.Indented );
            var recipientListJsonFileName = Path.Combine( reportRockStatementGeneratorTemporaryDirectory, "RecipientData.Json" );
            File.WriteAllText( recipientListJsonFileName, recipientListJson );
        }

        [Obsolete("TODO, this might not be needed")]
        private void SaveRecipientResults( IEnumerable<FinancialStatementGeneratorRecipientResult> recipientResultList, string reportRockStatementGeneratorTemporaryDirectory )
        {
            var recipientListResultJson = recipientResultList.ToJson( Formatting.None );
            var recipientListResultJsonFileName = Path.Combine( reportRockStatementGeneratorTemporaryDirectory, "RecipientResultsData.Json" );
            Stopwatch stopwatchWriteAllText = Stopwatch.StartNew();
            File.WriteAllText( recipientListResultJsonFileName, recipientListResultJson );
            Debug.WriteLine( $"stopwatchWriteAllText:{ stopwatchWriteAllText.Elapsed.TotalMilliseconds } ms" );
        }

        /// <summary>
        /// If there are some incomplete recipients, it verifies that the completed ones are really completed.
        /// </summary>
        public static void EnsureIncompletedSavedRecipientListCompletedStatus()
        {
            var rockStatementGeneratorTemporaryDirectory = GetStatementGeneratorTemporaryDirectory( RockConfig.Load(), DateTime.Today );
            var savedRecipientList = GetSavedRecipientList();
            if ( savedRecipientList == null )
            {
                // hasn't run
                return;
            }

            if ( !savedRecipientList.Any( a => a.IsComplete ) )
            {
                // if the whole thing is completed, there everything is all done
                return;
            }

            var savedResults = GetSavedRecipientResults();
            HashSet<string> savedResultKeys;
            if ( savedResults != null )
            {
                savedResultKeys = new HashSet<string>( savedResults.Select( a => a.Recipient.GetRecipientKey() ).ToList() );
            }
            else
            {
                savedResultKeys = new HashSet<string>();
            }

            // if there are some that are not complete, make sure the temp files haven't been cleaned up
            foreach ( var savedRecipient in savedRecipientList.Where( a => a.IsComplete ) )
            {
                if ( savedRecipient.GetPdfDocument( rockStatementGeneratorTemporaryDirectory ) == null )
                {
                    // if it was marked complete, but the temp file is gone, we'll have to re-do this recipient
                    savedRecipient.IsComplete = false;
                    continue;
                }

                if ( !savedResultKeys.Contains( savedRecipient.GetRecipientKey() ) )
                {
                    // if it was marked complete, but the result hasn't been saved, we'll have to re-do this recipient
                    savedRecipient.IsComplete = false;
                    continue;
                }
            }

            SaveRecipientListStatus( savedRecipientList.ToList(), rockStatementGeneratorTemporaryDirectory );
        }

        /// <summary>
        /// Gets the recipient list status.
        /// </summary>
        /// <returns></returns>
        public static List<FinancialStatementGeneratorRecipient> GetSavedRecipientList()
        {
            var rockConfig = RockConfig.Load();
            var fileName = Path.Combine( GetStatementGeneratorTemporaryDirectory( rockConfig, DateTime.Today ), "RecipientData.Json" );
            if ( File.Exists( fileName ) )
            {
                var resultsJson = File.ReadAllText( fileName );
                return resultsJson.FromJsonOrNull<List<FinancialStatementGeneratorRecipient>>();
            }

            return null;
        }

        /// <summary>
        /// Gets the saved recipient results.
        /// </summary>
        /// <returns></returns>
        private static List<FinancialStatementGeneratorRecipientResult> GetSavedRecipientResults()
        {
            var rockConfig = RockConfig.Load();
            var fileName = Path.Combine( GetStatementGeneratorTemporaryDirectory( rockConfig, DateTime.Today ), "RecipientResultsData.Json" );
            if ( File.Exists( fileName ) )
            {
                var resultsJson = File.ReadAllText( fileName );
                return resultsJson.FromJsonOrNull<List<FinancialStatementGeneratorRecipientResult>>();
            }

            return null;
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
        /// <param name="currentDate">The current date.</param>
        /// <returns></returns>
        private static string GetStatementGeneratorTemporaryDirectory( RockConfig rockConfig, DateTime currentDate )
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

            var reportRockStatementGeneratorTemporaryDirectory = Path.Combine( reportTemporaryDirectory, $"Rock Statement Generator-{currentDate.ToString( "MM_dd_yyyy" )}" );
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
        private static PdfPrintOptions GetPdfPrintOptions( FinancialStatementTemplatePDFSettings financialStatementTemplatePDFSettings, FinancialStatementGeneratorRecipientResult statementGeneratorRecipientResult )
        {
            PdfPrintOptions pdfPrintOptions = new IronPdf.PdfPrintOptions
            {
                MarginLeft = financialStatementTemplatePDFSettings.MarginLeftMillimeters ?? 10,
                MarginTop = financialStatementTemplatePDFSettings.MarginTopMillimeters ?? 10,
                MarginRight = financialStatementTemplatePDFSettings.MarginRightMillimeters ?? 10,
                MarginBottom = financialStatementTemplatePDFSettings.MarginBottomMillimeters ?? 10
            };

            switch ( financialStatementTemplatePDFSettings.PaperSize )
            {
                case FinancialStatementTemplatePDFSettingsPaperSize.A4:
                    pdfPrintOptions.PaperSize = PdfPrintOptions.PdfPaperSize.A4;
                    break;
                case FinancialStatementTemplatePDFSettingsPaperSize.Legal:
                    pdfPrintOptions.PaperSize = PdfPrintOptions.PdfPaperSize.Legal;
                    break;
                case FinancialStatementTemplatePDFSettingsPaperSize.Letter:
                default:
                    pdfPrintOptions.PaperSize = PdfPrintOptions.PdfPaperSize.Letter;
                    break;
            }

            // see https://ironpdf.com/examples/html-headers-and-footers/
            if ( statementGeneratorRecipientResult.FooterHtmlFragment.IsNotNullOrWhitespace() )
            {
                pdfPrintOptions.Footer = new HtmlHeaderFooter()
                {
                    HtmlFragment = statementGeneratorRecipientResult.FooterHtmlFragment
                };
            }

            return pdfPrintOptions;
        }

        /// <summary>
        /// Writes the group of statements to document.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="financialStatementGeneratorRecipientResults">The statement generator recipient PDF results.</param>
        /// <returns></returns>
        private void WriteStatementPDFs( FinancialStatementReportConfiguration financialStatementReportConfiguration, ConcurrentBag<FinancialStatementGeneratorRecipientResult> financialStatementGeneratorRecipientResults )
        {
            if ( !financialStatementGeneratorRecipientResults.Any() )
            {
                return;
            }

            var recipientList = financialStatementGeneratorRecipientResults.Where( a => a.Html != null ).ToList();

            if ( financialStatementReportConfiguration.ExcludeOptedOutIndividuals )
            {
                recipientList = recipientList.Where( a => a.Recipient.OptedOut == false ).ToList();
            }

            if ( financialStatementReportConfiguration.MinimumContributionAmount.HasValue )
            {
                recipientList = recipientList.Where( a => a.Recipient.ContributionTotal >= financialStatementReportConfiguration.MinimumContributionAmount.Value ).ToList();
            }

            if ( financialStatementReportConfiguration.IncludeInternationalAddresses == false )
            {
                recipientList = recipientList.Where( a => a.Recipient.IsInternationalAddress == false ).ToList();
            }

            IOrderedEnumerable<FinancialStatementGeneratorRecipientResult> sortedRecipientList = SortByPrimaryAndSecondaryOrder( financialStatementReportConfiguration, recipientList );
            recipientList = sortedRecipientList.ToList();

            var useChapters = financialStatementReportConfiguration.MaxStatementsPerChapter.HasValue;
            var splitOnPrimary = financialStatementReportConfiguration.SplitFilesOnPrimarySortValue;

            Dictionary<string, List<FinancialStatementGeneratorRecipientResult>> recipientsByPrimarySortKey;
            if ( financialStatementReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.PageCount )
            {
                recipientsByPrimarySortKey = recipientList
                    .GroupBy( k => k.Recipient.RenderedPageCount )
                    .ToDictionary( k => k.Key.ToString(), v => v.ToList() );
            }
            else if ( financialStatementReportConfiguration.PrimarySortOrder == FinancialStatementOrderBy.LastName )
            {
                recipientsByPrimarySortKey = recipientList
                    .GroupBy( k => k.Recipient.LastName )
                    .ToDictionary( k => k.Key, v => v.ToList() );
            }
            else
            {
                // group by postal code
                recipientsByPrimarySortKey = recipientList
                    .GroupBy( k => k.Recipient.PostalCode ?? "00000" )
                    .ToDictionary( k => k.Key, v => v.ToList() );
            }

            // make sure the directory exists
            Directory.CreateDirectory( financialStatementReportConfiguration.DestinationFolder );

            if ( splitOnPrimary )
            {
                foreach ( var primarySort in recipientsByPrimarySortKey )
                {
                    var primarySortFileName = $"{financialStatementReportConfiguration.FilenamePrefix}{primarySort.Key}.pdf".MakeValidFileName();
                    var sortedRecipients = SortByPrimaryAndSecondaryOrder( financialStatementReportConfiguration, primarySort.Value );
                    SaveToMergedDocument( Path.Combine( financialStatementReportConfiguration.DestinationFolder, primarySortFileName ), sortedRecipients.ToList() );
                }
            }
            else if ( useChapters )
            {
                SaveToChapters( financialStatementReportConfiguration, recipientsByPrimarySortKey );
            }
            else
            {
                var singleFileName = Path.Combine( financialStatementReportConfiguration.DestinationFolder, "statements.pdf" );
                SaveToMergedDocument( singleFileName, recipientList );

                return;
            }
        }

        /// <summary>
        /// Saves to chapters.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="recipientsByPrimarySortKey">The recipients by primary sort key.</param>
        private void SaveToChapters( FinancialStatementReportConfiguration financialStatementReportConfiguration, Dictionary<string, List<FinancialStatementGeneratorRecipientResult>> recipientsByPrimarySortKey )
        {
            var maxStatementsPerChapter = financialStatementReportConfiguration.MaxStatementsPerChapter.Value;
            List<FinancialStatementGeneratorRecipientResult> recipientsForChapter = new List<FinancialStatementGeneratorRecipientResult>();
            int chapterIndex = 1;
            string chapterStartPrimarySortKey = recipientsByPrimarySortKey.Keys.FirstOrDefault();
            string currentPrimarySortKey = chapterStartPrimarySortKey;

            foreach ( var primarySort in recipientsByPrimarySortKey )
            {
                currentPrimarySortKey = primarySort.Key;
                if ( financialStatementReportConfiguration.PreventSplittingPrimarySortValuesAcrossChapters )
                {
                    recipientsForChapter.AddRange( primarySort.Value );

                    if ( recipientsForChapter.Count >= maxStatementsPerChapter )
                    {
                        SaveChapterDoc( financialStatementReportConfiguration, chapterIndex, chapterStartPrimarySortKey, currentPrimarySortKey, recipientsForChapter );
                        chapterStartPrimarySortKey = primarySort.Key;
                        recipientsForChapter = new List<FinancialStatementGeneratorRecipientResult>();
                        chapterIndex++;
                    }
                }
                else
                {
                    foreach ( var recipient in primarySort.Value )
                    {
                        recipientsForChapter.Add( recipient );

                        if ( recipientsForChapter.Count >= maxStatementsPerChapter )
                        {
                            SaveChapterDoc( financialStatementReportConfiguration, chapterIndex, chapterStartPrimarySortKey, currentPrimarySortKey, recipientsForChapter );
                            chapterStartPrimarySortKey = primarySort.Key;
                            recipientsForChapter = new List<FinancialStatementGeneratorRecipientResult>();
                            chapterIndex++;
                        }
                    }
                }
            }

            if ( recipientsForChapter.Any() )
            {
                SaveChapterDoc( financialStatementReportConfiguration, chapterIndex, chapterStartPrimarySortKey, currentPrimarySortKey, recipientsForChapter );
            }
        }

        /// <summary>
        /// Saves the chapter document.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="chapterIndex">Index of the chapter.</param>
        /// <param name="chapterStartPrimarySortKey">The chapter start primary sort key.</param>
        /// <param name="chapterEndPrimarySortKey">The chapter end primary sort key.</param>
        /// <param name="statementsForChapter">The statements for chapter.</param>
        private void SaveChapterDoc( FinancialStatementReportConfiguration financialStatementReportConfiguration, int chapterIndex, string chapterStartPrimarySortKey, string chapterEndPrimarySortKey, List<FinancialStatementGeneratorRecipientResult> statementsForChapter )
        {
            var primarySortRange = $"{chapterStartPrimarySortKey}-{chapterEndPrimarySortKey}";
            var chapterFileName = $"{financialStatementReportConfiguration.FilenamePrefix}{primarySortRange}-{chapterIndex}.pdf".MakeValidFileName();
            var sortedRecipients = SortByPrimaryAndSecondaryOrder( financialStatementReportConfiguration, statementsForChapter );
            SaveToMergedDocument( Path.Combine( financialStatementReportConfiguration.DestinationFolder, chapterFileName ), sortedRecipients.ToList() );
        }

        /// <summary>
        /// Saves to merged document.
        /// </summary>
        /// <param name="mergedFileName">Name of the merged file.</param>
        /// <param name="recipientList">The recipient list.</param>
        private void SaveToMergedDocument( string mergedFileName, List<FinancialStatementGeneratorRecipientResult> recipientList )
        {
            // no chapters, so just write all to one single document
            double mergeTotal = recipientList.Count();
            double mergeProgress = 0;

            var allPdfsEnumerable = recipientList.Select( a =>
            {
                mergeProgress++;
                Debug.WriteLine( $"Merging {mergeProgress / mergeTotal}" );
                return a.Recipient.GetPdfDocument( _currentDayTemporaryDirectory );
            } );

            var singleFinalDoc = IronPdf.PdfDocument.Merge( allPdfsEnumerable );
            singleFinalDoc.SaveAs( mergedFileName );
        }

        /// <summary>
        /// Sorts the by primary and secondary order.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        /// <param name="recipientList">The recipient list.</param>
        /// <returns></returns>
        private static IOrderedEnumerable<FinancialStatementGeneratorRecipientResult> SortByPrimaryAndSecondaryOrder( FinancialStatementReportConfiguration financialStatementReportConfiguration, List<FinancialStatementGeneratorRecipientResult> recipientList )
        {
            IOrderedEnumerable<FinancialStatementGeneratorRecipientResult> sortedRecipientList;

            switch ( financialStatementReportConfiguration.PrimarySortOrder )
            {
                case FinancialStatementOrderBy.PageCount:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.Recipient.RenderedPageCount );
                        break;
                    }

                case FinancialStatementOrderBy.PostalCode:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.Recipient.PostalCode );
                        break;
                    }

                case FinancialStatementOrderBy.LastName:
                default:
                    {
                        sortedRecipientList = recipientList.OrderBy( a => a.Recipient.LastName ).ThenBy( a => a.Recipient.NickName );
                        break;
                    }
            }

            switch ( financialStatementReportConfiguration.SecondarySortOrder )
            {
                case FinancialStatementOrderBy.PageCount:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.Recipient.RenderedPageCount );
                        break;
                    }

                case FinancialStatementOrderBy.PostalCode:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.Recipient.PostalCode );
                        break;
                    }

                case FinancialStatementOrderBy.LastName:
                default:
                    {
                        sortedRecipientList = sortedRecipientList.ThenBy( a => a.Recipient.LastName ).ThenBy( a => a.Recipient.NickName );
                        break;
                    }
            }

            return sortedRecipientList;
        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progressMessage">The message.</param>
        /// <param name="position">The position.</param>
        /// <param name="max">The maximum.</param>
        private void UpdateProgress( string progressMessage, long position, int max )
        {
            OnProgress?.Invoke( this, new ProgressEventArgs { ProgressMessage = progressMessage, Position = ( int ) position, Max = max } );
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
    internal static class FinancialStatementGeneratorRecipientExtensions
    {
        /// <summary>
        /// Gets the PDF document.
        /// </summary>
        /// <param name="financialStatementGeneratorRecipientResult">The financial statement generator recipient result.</param>
        /// <param name="reportRockStatementGeneratorStatementsTemporaryDirectory">The report rock statement generator statements temporary directory.</param>
        /// <returns></returns>
        internal static PdfDocument GetPdfDocument( this FinancialStatementGeneratorRecipient recipient, string reportRockStatementGeneratorStatementsTemporaryDirectory )
        {
            var filePath = recipient.GetPdfDocumentFilePath( reportRockStatementGeneratorStatementsTemporaryDirectory );
            if ( File.Exists( filePath ) )
            {
                return PdfDocument.FromFile( filePath );
            }

            return null;
        }

        /// <summary>
        /// Gets the recipient key.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <returns></returns>
        internal static string GetRecipientKey( this FinancialStatementGeneratorRecipient recipient )
        {
            return $"GroupId_{recipient.GroupId}_PersonID_{recipient.PersonId}";
        }

        /// <summary>
        /// Gets the PDF document file path.
        /// </summary>
        /// <param name="financialStatementGeneratorRecipientResult">The financial statement generator recipient result.</param>
        /// <param name="currentDayTemporaryDirectory">The current day temporary directory.</param>
        /// <returns></returns>
        internal static string GetPdfDocumentFilePath( this FinancialStatementGeneratorRecipient recipient, string currentDayTemporaryDirectory )
        {
            string pdfTempFileName = $"{GetRecipientKey( recipient )}.pdf";
            return Path.Combine( currentDayTemporaryDirectory, "Statements", pdfTempFileName );
        }
    }
}
