using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ReportSettings.xaml
    /// </summary>
    public partial class ReportSettings : System.Windows.Controls.Page
    {
        public ReportSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnShowReportSettingsModal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnShowReportSettingsModal_Click( object sender, RoutedEventArgs e )
        {
            FinancialStatementReportConfiguration selectedSettings = null;
            ReportConfigurationModalWindow reportSettingsModalWindow = new ReportConfigurationModalWindow( selectedSettings );
            reportSettingsModalWindow.Owner = Window.GetWindow( this );
            var showDialogResult = reportSettingsModalWindow.ShowDialog();
            if ( showDialogResult == true )
            {
                FinancialStatementReportConfiguration updatedSettings = reportSettingsModalWindow.GetFinancialStatementReportConfiguration();
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
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
    }
}
