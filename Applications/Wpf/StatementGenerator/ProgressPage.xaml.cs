﻿// <copyright>
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;

using Rock.Wpf;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ProgressPage.xaml
    /// </summary>
    public partial class ProgressPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressPage"/> class.
        /// </summary>
        public ProgressPage()
        {
            InitializeComponent();
        }

        private void NavigationService_Navigating( object sender, System.Windows.Navigation.NavigatingCancelEventArgs e )
        {
            // if the currently running, don't let navigation happen. This fixes an issue where pressing the BackSpace key would go to previous page
            // even though the report was still running
            e.Cancel = _isRunning;
        }

        /// <summary>
        /// The statement count
        /// </summary>
        private int _statementCount;

        private static bool _wasCancelled = false;
        private static bool _isRunning = false;

        private ContributionReport _contributionReport;

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            var window = Window.GetWindow( this );
            window.KeyDown += Window_KeyDown;

            NavigationService.Navigating += NavigationService_Navigating;
            btnPrev.Visibility = Visibility.Hidden;
            lblReportProgress.Visibility = Visibility.Hidden;
            lblReportProgress.Content = "Progress - Creating Statements";
            lblReportProgress.Content = "Progress - Creating Statements";
            pgReportProgress.Visibility = Visibility.Hidden;
            WpfHelper.FadeIn( pgReportProgress, 2000 );
            WpfHelper.FadeIn( lblReportProgress, 2000 );
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void Window_KeyDown( object sender, KeyEventArgs e )
        {
            bool isLeftAltDown = Keyboard.IsKeyDown( Key.LeftAlt );
            bool isDeleteDown = Keyboard.IsKeyDown( Key.Delete ) || Keyboard.IsKeyDown( Key.Back );
            if ( isLeftAltDown && isDeleteDown )
            {
                _contributionReport?.Cancel();

                var window = Window.GetWindow( this );
                window.KeyDown -= Window_KeyDown;
            }
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            btnPrev.Visibility = Visibility.Visible;
            pgReportProgress.Visibility = Visibility.Collapsed;

            if ( e.Error != null )
            {
                lblReportProgress.Content = "Error: " + e.Error.Message;
                throw e.Error;
            }

            if ( _statementCount == 0 )
            {
                lblReportProgress.Content = @"Warning: No records matched your criteria. No statements have been created.";
            }
            else if ( _wasCancelled )
            {
                lblReportProgress.Style = this.FindResource( "labelStyleAlertWarning" ) as Style;
                lblReportProgress.Content = $@"Canceled: {_statementCount} statements created.";
            }
            else
            {
                lblReportProgress.Style = this.FindResource( "labelStyleAlertSuccess" ) as Style;
                lblReportProgress.Content = string.Format( @"Success:{1}Your statements have been created.{1}( {0} statements created )", _statementCount, Environment.NewLine );
            }
        }

        /// <summary>
        /// Handles the DoWork event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            _contributionReport = new ContributionReport( ReportOptions.Current );
            _contributionReport.OnProgress += ContributionReport_OnProgress;
            _wasCancelled = false;
            _isRunning = true;
            _statementCount = _contributionReport.RunReport();
            _isRunning = false;
            _wasCancelled = _contributionReport.IsCancelled;

            _contributionReport = null;

            e.Result = _statementCount > 0;
        }

        /// <summary>
        /// Handles the OnProgress event of the ContributionReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressEventArgs"/> instance containing the event data.</param>
        private void ContributionReport_OnProgress( object sender, ProgressEventArgs e )
        {
            ShowProgress( e.Position, e.Max, e.ProgressMessage );
        }

        /// <summary>
        /// The _start progress date time
        /// </summary>
        private DateTime _lastUpdate = DateTime.MinValue;

        /// <summary>
        /// Shows the progress.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="progressMessage">The progress message.</param>
        private void ShowProgress( int position, int max, string progressMessage )
        {
            var timeSinceLastUpdate = DateTime.Now - _lastUpdate;

            if ( timeSinceLastUpdate.Seconds < 1.0 && position != max )
            {
                return;
            }

            _lastUpdate = DateTime.Now;

            Dispatcher.Invoke( () =>
            {
                if ( max > 0 )
                {
                    if ( lblReportProgress.Content.ToString() != progressMessage )
                    {
                        lblReportProgress.Content = progressMessage;
                    }

                    if ( pgReportProgress.Maximum != max )
                    {
                        pgReportProgress.Maximum = max;
                    }

                    if ( pgReportProgress.Value != position )
                    {
                        pgReportProgress.Value = position;
                    }

                    if ( pgReportProgress.Visibility != Visibility.Visible )
                    {
                        pgReportProgress.Visibility = Visibility.Visible;
                    }

                    // put the current statements/second in stats box (easter egg)
                    var duration = DateTime.Now - ContributionReport.StartDateTime;
                    if ( duration.TotalSeconds > 1 )
                    {
                        double rate = ContributionReport.RecordsCompletedCount / duration.TotalSeconds;
                        string statsText = $"{position}/{max} @ {rate:F2} per second";
                        if ( ( string ) lblStats.Content != statsText )
                        {
                            lblStats.Content = statsText;
                        }
                    }
                }
                else
                {
                    lblReportProgress.Content = progressMessage;
                    pgReportProgress.Visibility = Visibility.Collapsed;
                }
            } );
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the lblReportProgress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void lblReportProgress_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            if ( lblStats.Visibility != Visibility.Visible )
            {
                lblStats.Visibility = Visibility.Visible;
            }
            else
            {
                lblStats.Visibility = Visibility.Hidden;
            }
        }
    }
}
