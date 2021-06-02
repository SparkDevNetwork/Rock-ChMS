using System.Collections.Generic;

namespace Rock.Apps.StatementGenerator
{
    public class ResultsSummary
    {
        public int StatementCount => NumberOfGivingUnits;
        public int NumberOfGivingUnits { get; set; }
        public decimal TotalAmount { get; set; }
        public int? PaperlessStatementsCount { get; set; }
        public decimal? PaperlessStatementTotalAmount { get; set; }
        public int? PaperlessStatementsIndividualCount { get; set; }
        public List<ReportPaperStatementsSummary> PaperStatementsSummaryList { get; set; } = new List<ReportPaperStatementsSummary>();
    }

    public class ReportPaperStatementsSummary
    {
        public string PrimarySortName { get; set; }
        public int NumberOfStatements { get; set; }
        public decimal TotalAmount { get; set; }

        public System.Windows.Visibility StatementsExcludedMinAmountVisibility { get; set; } = System.Windows.Visibility.Collapsed;
        public string StatementsExcludedMinAmountLabel { get; set; }
        public int StatementsExcludedMinAmountSummary { get; set; }

        public System.Windows.Visibility StatementsExcludedInternationalVisibility { get; set; } = System.Windows.Visibility.Collapsed;
        public int StatementsExcludedInternationalSummary { get; set; }

        public System.Windows.Visibility StatementsExcludedIncompleteAddressVisibility { get; set; } = System.Windows.Visibility.Collapsed;
        public int StatementsExcludedIncompleteAddressSummary { get; set; }
    }
}
