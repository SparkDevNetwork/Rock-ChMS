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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ReportSettings.xaml
    /// </summary>
    public partial class ReportSettings : System.Windows.Controls.Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportSettings"/> class.
        /// </summary>
        public ReportSettings()
        {
            InitializeComponent();

            cbEnablePageCountPredetermination.IsChecked = RockConfig.Load().EnablePageCountPredetermination;

            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockConfig = RockConfig.Load();

            List<FinancialStatementReportConfiguration> reportConfigurationList = null;

            try
            {
                if ( rockConfig.ReportConfigurationListJson.IsNotNullOrWhitespace() )
                {
                    reportConfigurationList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FinancialStatementReportConfiguration>>( rockConfig.ReportConfigurationListJson );
                }
            }
            catch
            {
                // ignore
            }

            reportConfigurationList = reportConfigurationList ?? new List<FinancialStatementReportConfiguration>();
            ReportOptions.Current.ReportConfigurationList = reportConfigurationList;

            grdReportSettings.DataContext = reportConfigurationList.OrderBy( a => a.CreatedDateTime );
        }

        /// <summary>
        /// Handles the Click event of the btnShowReportSettingsModal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnShowReportSettingsModal_Click( object sender, RoutedEventArgs e )
        {
            AddEditReportSettings( null );
        }

        /// <summary>
        /// Adds the edit report settings.
        /// </summary>
        /// <param name="selectedSettings">The selected settings.</param>
        private void AddEditReportSettings( FinancialStatementReportConfiguration selectedSettings )
        {
            ReportConfigurationModalWindow reportSettingsModalWindow = new ReportConfigurationModalWindow( selectedSettings );
            reportSettingsModalWindow.Owner = Window.GetWindow( this );
            var showDialogResult = reportSettingsModalWindow.ShowDialog();
            if ( showDialogResult == true )
            {
                FinancialStatementReportConfiguration updatedSettings = reportSettingsModalWindow.GetFinancialStatementReportConfiguration();
                ReportOptions.Current.ReportConfigurationList = ReportOptions.Current.ReportConfigurationList ?? new List<FinancialStatementReportConfiguration>();
                var settingsToUpdate = ReportOptions.Current.ReportConfigurationList.FirstOrDefault( a => a.Guid == updatedSettings.Guid );
                if ( settingsToUpdate != null )
                {
                    // replace the settings with the new ones
                    ReportOptions.Current.ReportConfigurationList.Remove( settingsToUpdate );
                }

                ReportOptions.Current.ReportConfigurationList.Add( updatedSettings );

                var rockConfig = RockConfig.Load();
                rockConfig.ReportConfigurationListJson = Newtonsoft.Json.JsonConvert.SerializeObject( ReportOptions.Current.ReportConfigurationList );
                rockConfig.Save();

                BindGrid();
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            RockConfig.Load().EnablePageCountPredetermination = cbEnablePageCountPredetermination.IsChecked == true;
            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            if ( SaveChanges( true ) )
            {
                var nextPage = new ProgressPage();
                this.NavigationService.Navigate( nextPage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            SaveChanges( false );
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteReportOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteReportOption_Click( object sender, RoutedEventArgs e )
        {
            ReportOptions.Current.ReportConfigurationList = ReportOptions.Current.ReportConfigurationList ?? new List<FinancialStatementReportConfiguration>();
            var seletedReportConfig = ( sender as Button ).DataContext as FinancialStatementReportConfiguration;
            ReportOptions.Current.ReportConfigurationList.Remove( seletedReportConfig );

            var rockConfig = RockConfig.Load();
            rockConfig.ReportConfigurationListJson = Newtonsoft.Json.JsonConvert.SerializeObject( ReportOptions.Current.ReportConfigurationList );
            rockConfig.Save();

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDoubleClick event of the grdReportSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void grdReportSettings_RowDoubleClick( object sender, MouseButtonEventArgs e )
        {
            var seletedReportConfig = ( sender as DataGridRow ).DataContext as FinancialStatementReportConfiguration;
            AddEditReportSettings( seletedReportConfig );
        }
    }
}
