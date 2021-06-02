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

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ResultsSummaryPage.xaml
    /// </summary>
    public partial class ResultsSummaryPage : Page
    {
        public ResultsSummaryPage( ResultsSummary resultsSummary )
        {
            InitializeComponent();

            resultsSummary = resultsSummary ?? new ResultsSummary();

            lblNumberOfGivingUnits.Content = resultsSummary.NumberOfGivingUnits;
            lblTotalGivingAmount.Content = resultsSummary.TotalAmount;
            lblNumberOfPaperlessStatements.Content = resultsSummary.PaperlessStatementsCount;
            lblPaperlessStatementsTotalAmount.Content = resultsSummary.PaperlessStatementTotalAmount;
            lblPaperlessStatementsNumberOfIndividuals.Content = resultsSummary.PaperlessStatementsIndividualCount;

            rptReportStatistics.DataContext = resultsSummary.PaperStatementsSummaryList;
        }
    }
}
