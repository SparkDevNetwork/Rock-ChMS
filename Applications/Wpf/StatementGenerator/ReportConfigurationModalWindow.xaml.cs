using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Rock.Client;
using Rock.Client.Enums;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ReportConfigurationModalWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfigurationModalWindow"/> class.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        public ReportConfigurationModalWindow( FinancialStatementReportConfiguration financialStatementReportConfiguration )
        {
            InitializeComponent();

            if ( financialStatementReportConfiguration == null)
            {
                lblActionTitle.Content = "Add Report";
                financialStatementReportConfiguration = new FinancialStatementReportConfiguration();
            }
            else
            {
                lblActionTitle.Content = "Edit Report";
            }

            cboPrimarySort.Items.Clear();
            var orderByOptions = Enum.GetValues( typeof( FinancialStatementOrderBy ) ).OfType<FinancialStatementOrderBy>();
            foreach ( var orderByOption in orderByOptions )
            {
                var primarySortItem = new ComboBoxItem { Content = orderByOption.ConvertToString( true ), Tag = orderByOption };
                primarySortItem.IsSelected = financialStatementReportConfiguration.PrimarySortOrder == orderByOption;
                cboPrimarySort.Items.Add( primarySortItem );

                var secondarySortItem = new ComboBoxItem { Content = orderByOption.ConvertToString( true ), Tag = orderByOption };
                secondarySortItem.IsSelected = financialStatementReportConfiguration.SecondarySortOrder == orderByOption;
                cboSecondarySort.Items.Add( secondarySortItem );
            }

            tbDestinationFolder.Text = financialStatementReportConfiguration.DestinationFolder;
            tbFilenamePrefix.Text = financialStatementReportConfiguration.FilenamePrefix;
            cbSplitFilesOnPrimarySortValue.IsChecked = financialStatementReportConfiguration.SplitFilesOnPrimarySortValue;
            tbMaxStatementsInChapter.Text = financialStatementReportConfiguration.MaxStatementsPerChapter.ToString();
            cbPreventSplittingPrimarySortValuesAcrossChapters.IsChecked = financialStatementReportConfiguration.PreventSplittingPrimarySortValuesAcrossChapters;
            tbMinimumContributionAmount.Text = financialStatementReportConfiguration.MinimumContributionAmount.ToString();
            cbIncludeInternationalAddresses.IsChecked = financialStatementReportConfiguration.IncludeInternationalAddresses;
            cbDoNotIncludeStatementsForThoseWhoHaveOptedOut.IsChecked = financialStatementReportConfiguration.ExcludeOptedOutIndividuals;
        }

        /// <summary>
        /// Gets the financial statement report configuration.
        /// </summary>
        /// <returns></returns>
        public FinancialStatementReportConfiguration GetFinancialStatementReportConfiguration()
        {
            FinancialStatementReportConfiguration financialStatementReportConfiguration = new FinancialStatementReportConfiguration();
            financialStatementReportConfiguration.PrimarySortOrder = ( FinancialStatementOrderBy? ) cboPrimarySort.SelectedValue ?? FinancialStatementOrderBy.PostalCode;
            financialStatementReportConfiguration.SecondarySortOrder = ( FinancialStatementOrderBy? ) cboSecondarySort.SelectedValue ?? FinancialStatementOrderBy.LastName;
            financialStatementReportConfiguration.DestinationFolder = tbDestinationFolder.Text;
            financialStatementReportConfiguration.FilenamePrefix = tbFilenamePrefix.Text;
            financialStatementReportConfiguration.SplitFilesOnPrimarySortValue = cbSplitFilesOnPrimarySortValue.IsChecked ?? false;
            financialStatementReportConfiguration.MaxStatementsPerChapter = tbMaxStatementsInChapter.Text.AsIntegerOrNull();
            financialStatementReportConfiguration.PreventSplittingPrimarySortValuesAcrossChapters = cbPreventSplittingPrimarySortValuesAcrossChapters.IsChecked ?? true;
            financialStatementReportConfiguration.MinimumContributionAmount = tbMinimumContributionAmount.Text.AsDecimalOrNull();
            financialStatementReportConfiguration.IncludeInternationalAddresses = cbIncludeInternationalAddresses.IsChecked ?? true;
            financialStatementReportConfiguration.ExcludeOptedOutIndividuals = cbDoNotIncludeStatementsForThoseWhoHaveOptedOut.IsChecked ?? true;

            return financialStatementReportConfiguration;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            // ToDo
            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnSaveChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSaveChanges_Click( object sender, RoutedEventArgs e )
        {
            if ( SaveChanges( true ) )
            {
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }
    }
}
