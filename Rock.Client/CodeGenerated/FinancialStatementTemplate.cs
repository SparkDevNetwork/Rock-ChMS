//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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


namespace Rock.Client
{
    /// <summary>
    /// Base client model for FinancialStatementTemplate that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class FinancialStatementTemplateEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public string Description { get; set; }

        /// <summary />
        public string FooterSettingsJSON { get; set; } = @"{""LeftTemplate"":null,""CenterTemplate"":null,""RightTemplate"":null}";

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public bool IsActive { get; set; } = true;

        /// <summary />
        public int? LogoBinaryFileId { get; set; }

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public string Name { get; set; }

        /// <summary />
        public string ReportSettingsJson { get; set; } = @"{""TransactionSettings"":{""AccountSelectionOption"":0,""SelectedAccountIds"":[],""CurrencyTypesForCashGiftIds"":[],""CurrencyTypesForNonCashIds"":[],""TransactionTypeIds"":[],""HideRefundedTransactions"":false,""HideCorrectedTransactionOnSameData"":false},""PledgeSettings"":{""AccountIds"":[],""IncludeGiftsToChildAccounts"":false,""IncludeNonCashGifts"":false},""PDFObjectSettings"":{}}";

        /// <summary />
        public string ReportTemplate { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// This does not need to be set or changed. Rock will always set this to the current date/time when saved to the database.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// If you need to set this manually, set ModifiedAuditValuesAlreadyUpdated=True to prevent Rock from setting it
        /// </summary>
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary />
        public Guid Guid { get; set; }

        /// <summary />
        public int? ForeignId { get; set; }

        /// <summary>
        /// Copies the base properties from a source FinancialStatementTemplate object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( FinancialStatementTemplate source )
        {
            this.Id = source.Id;
            this.Description = source.Description;
            this.FooterSettingsJSON = source.FooterSettingsJSON;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.IsActive = source.IsActive;
            this.LogoBinaryFileId = source.LogoBinaryFileId;
            this.ModifiedAuditValuesAlreadyUpdated = source.ModifiedAuditValuesAlreadyUpdated;
            this.Name = source.Name;
            this.ReportSettingsJson = source.ReportSettingsJson;
            this.ReportTemplate = source.ReportTemplate;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for FinancialStatementTemplate that includes all the fields that are available for GETs. Use this for GETs (use FinancialStatementTemplateEntity for POST/PUTs)
    /// </summary>
    public partial class FinancialStatementTemplate : FinancialStatementTemplateEntity
    {
        /// <summary />
        public BinaryFile LogoBinaryFile { get; set; }

        /// <summary>
        /// NOTE: Attributes are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.Attribute> Attributes { get; set; }

        /// <summary>
        /// NOTE: AttributeValues are only populated when ?loadAttributes is specified. Options for loadAttributes are true, false, 'simple', 'expanded' 
        /// </summary>
        public Dictionary<string, Rock.Client.AttributeValue> AttributeValues { get; set; }
    }
}
