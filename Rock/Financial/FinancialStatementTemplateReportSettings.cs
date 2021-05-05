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

using Rock.Data;

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
            AccountIdsCustom = new List<int>();
            CurrencyTypesForCashGiftIds = new List<int>();
            CurrencyTypesForNonCashIds = new List<int>();
            TransactionTypeIds = new List<int>();
        }

        /// <summary>
        /// Gets or sets the account identifiers.
        /// </summary>
        /// <value>
        /// The account identifiers.
        /// </value>
        public List<int> AccountIdsCustom { get; set; }

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
}
