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
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialStatementTemplateReportSettings"/> class.
    /// </summary>
    [Serializable]
    [RockClientInclude( "Report Settings related to the Statement Generator" )]
    public class FinancialStatementTemplateReportSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialStatementTemplateReportSettings"/> class.
        /// </summary>
        public FinancialStatementTemplateReportSettings()
        {
            this.TransactionSettings = new FinancialStatementTemplateTransactionSetting();
            this.PledgeSettings = new FinancialStatementTemplatePledgeSettings();
            this.PDFObjectSettings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the transaction settings.
        /// </summary>
        /// <value>
        /// The transaction settings.
        /// </value>
        public FinancialStatementTemplateTransactionSetting TransactionSettings { get; set; }

        /// <summary>
        /// Gets or sets the pledge settings.
        /// </summary>
        /// <value>
        /// The pledge settings.
        /// </value>
        public FinancialStatementTemplatePledgeSettings PledgeSettings { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of Key,Value for PDF Object Settings.
        /// </summary>
        /// <value>
        /// The Dictionary of Key,Value for PDF Object Settings.
        /// </value>
        public Dictionary<string, string> PDFObjectSettings { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FinancialStatementTemplateTransactionSettingAccountSelectionOption
    {
        /// <summary>
        /// All tax deductible accounts
        /// </summary>
        AllTaxDeductibleAccounts = 0,

        /// <summary>
        /// The selected accounts
        /// </summary>
        SelectedAccounts = 1,

        /// <summary>
        /// The selected accounts include children
        /// </summary>
        SelectedAccountsIncludeChildren = 2
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialStatementTemplateTransactionSetting"/> class.
    /// </summary>
    [RockClientInclude( "Transaction Settings related to the Statement Generator" )]
    public class FinancialStatementTemplateTransactionSetting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialStatementTemplateTransactionSetting"/> class.
        /// </summary>
        public FinancialStatementTemplateTransactionSetting()
        {
            SelectedAccountIds = new List<int>();
            CurrencyTypesForCashGiftIds = new List<int>();
            CurrencyTypesForNonCashIds = new List<int>();
            TransactionTypeIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the account selection option.
        /// </summary>
        /// <value>
        /// The account selection option.
        /// </value>
        public FinancialStatementTemplateTransactionSettingAccountSelectionOption AccountSelectionOption { get; set; }

        /// <summary>
        /// Gets or sets the selected account ids.
        /// Use these if <see cref="AccessViolationException" /> is
        /// <see cref="FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccounts" />  or
        /// <see cref="FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccountsIncludeChildren" />  or
        /// </summary>
        /// <value>
        /// The selected account ids.
        /// </value>
        public List<int> SelectedAccountIds { get; set; }

        /// <summary>
        /// Gets or sets the currency types for cash gifts.
        /// </summary>
        /// <value>
        /// The currency types for cash gifts.
        /// </value>
        public List<int> CurrencyTypesForCashGiftIds { get; set; }

        /// <summary>
        /// Gets or sets the currency types for non-cash gifts.
        /// </summary>
        /// <value>
        /// The currency types for non-cash gifts.
        /// </value>
        public List<int> CurrencyTypesForNonCashIds { get; set; }

        /// <summary>
        /// Gets or sets the transaction types.
        /// </summary>
        /// <value>
        /// The transaction types.
        /// </value>
        public List<int> TransactionTypeIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether refunded transaction should be hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if refunded transaction should be hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideRefundedTransactions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether corrected transaction on same date should be hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if corrected transaction on same date should be hidden; otherwise, <c>false</c>.
        /// </value>
        public bool HideCorrectedTransactionOnSameData { get; set; }

        /// <summary>
        /// Gets the included account ids based on the <see cref="AccountSelectionOption" />
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public int[] GetIncludedAccountIds( RockContext rockContext )
        {
            List<int> transactionAccountIds;
            if ( AccountSelectionOption == FinancialStatementTemplateTransactionSettingAccountSelectionOption.AllTaxDeductibleAccounts )
            {
                transactionAccountIds = new FinancialAccountService( rockContext ).Queryable().Where( a => a.IsTaxDeductible ).Select( a => a.Id ).ToList();
            }
            else
            {
                transactionAccountIds = this.SelectedAccountIds ?? new List<int>();
                if ( this.AccountSelectionOption == FinancialStatementTemplateTransactionSettingAccountSelectionOption.SelectedAccountsIncludeChildren )
                {
                    var childAccountIds = new FinancialAccountService( rockContext ).Queryable()
                        .Where( a => a.ParentAccountId.HasValue && transactionAccountIds.Contains( a.ParentAccountId.Value ) )
                        .Select( a => a.Id ).ToList();
                    transactionAccountIds.AddRange( childAccountIds );
                }
            }

            return transactionAccountIds.ToArray();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FinancialStatementTemplatePledgeSettings"/> class.
    /// </summary>
    [RockClientInclude( "Pledge Settings related to the Statement Generator" )]
    public class FinancialStatementTemplatePledgeSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialStatementTemplatePledgeSettings"/> class.
        /// </summary>
        public FinancialStatementTemplatePledgeSettings()
        {
            AccountIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the account identifiers.
        /// </summary>
        /// <value>
        /// The account identifiers.
        /// </value>
        public List<int> AccountIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gifts to child accounts should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include gifts to child accounts]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeGiftsToChildAccounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether non-cash gifts should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include non-cash gifts]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeNonCashGifts { get; set; }
    }

    /// <summary>
    /// See https://ironpdf.com/object-reference/api/IronPdf.SimpleHeaderFooter.html
    /// </summary>
    [RockClientInclude( "Header/Footer Settings related to the Statement Generator. See https://ironpdf.com/object-reference/api/IronPdf.SimpleHeaderFooter.html" )]
    public class FinancialStatementTemplateHeaderFooterSettings
    {

        /// <summary>
        /// Sets the left hand side header text for the PDF document.
        /// Merge meta-data into your header using any of these placeholder strings: {page}
        /// {total-pages} {url} {date} {time} {html-title} {pdf-title}
        /// </summary>
        public string LeftTemplate;

        /// <summary>
        ///  Sets the centered header text for the PDF document.
        ///  Merge meta-data into your header using any of these placeholder strings: {page}
        ///  {total-pages} {url} {date} {time} {html-title} {pdf-title}
        /// </summary>
        public string CenterTemplate;

        /// <summary>
        /// Sets the right hand side header text for the PDF document.
        /// Merge meta-data into your header using any of these placeholder strings: {page}
        /// {total-pages} {url} {date} {time} {html-title} {pdf-title}
        /// </summary>
        public string RightTemplate;
    }
}
