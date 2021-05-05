using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Financial;

namespace Rock.StatementGenerator
{
    /// <summary>
    /// Request Body for api/FinancialGivingStatement/UploadGivingStatementDocument
    /// </summary>
    [RockClientInclude( "Request Body for api/FinancialGivingStatement/UploadGivingStatementDocument" )]
    public class UploadGivingStatementData
    {
        /// <summary>
        /// Gets or sets the financial statement individual save options.
        /// </summary>
        /// <value>
        /// The financial statement individual save options.
        /// </value>
        public FinancialStatementGeneratorOptions.FinancialStatementIndividualSaveOptions FinancialStatementIndividualSaveOptions { get; set; }

        /// <summary>
        /// The financial statement generator recipient
        /// </summary>
        public FinancialStatementGeneratorRecipient FinancialStatementGeneratorRecipient;

        /// <summary>
        /// Gets or sets the PDF data.
        /// </summary>
        /// <value>
        /// The PDF data.
        /// </value>
        public byte[] PDFData { get; set; }
    }
}
