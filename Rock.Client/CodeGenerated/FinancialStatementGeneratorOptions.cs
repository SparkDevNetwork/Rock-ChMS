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
    /// The FinancialStatementGeneratorOptions that are configured on the Statement Generator WPF application
    /// </summary>
    public partial class FinancialStatementGeneratorOptionsEntity
    {
        /// <summary />
        public int? DataViewId { get; set; }

        /// <summary />
        public bool EnablePageCountPredetermination { get; set; }

        /// <summary />
        public DateTime? EndDate { get; set; }

        /// <summary />
        public bool ExcludeInActiveIndividuals { get; set; }

        /// <summary />
        public int? FinancialStatementTemplateId { get; set; }

        /// <summary />
        public bool IncludeBusinesses { get; set; } = true;

        /// <summary />
        public bool IncludeIndividualsWithNoAddress { get; set; }

        /// <summary />
        public FinancialStatementIndividualSaveOptions IndividualSaveOptions { get; set; }

        /// <summary />
        public int? PersonId { get; set; }

        /// <summary />
        public List<FinancialStatementReportConfiguration> ReportConfigurationList { get; set; }

        /// <summary />
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Copies the base properties from a source FinancialStatementGeneratorOptions object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( FinancialStatementGeneratorOptions source )
        {
            this.DataViewId = source.DataViewId;
            this.EnablePageCountPredetermination = source.EnablePageCountPredetermination;
            this.EndDate = source.EndDate;
            this.ExcludeInActiveIndividuals = source.ExcludeInActiveIndividuals;
            this.FinancialStatementTemplateId = source.FinancialStatementTemplateId;
            this.IncludeBusinesses = source.IncludeBusinesses;
            this.IncludeIndividualsWithNoAddress = source.IncludeIndividualsWithNoAddress;
            this.IndividualSaveOptions = source.IndividualSaveOptions;
            this.PersonId = source.PersonId;
            this.ReportConfigurationList = source.ReportConfigurationList;
            this.StartDate = source.StartDate;

        }
    }

    /// <summary>
    /// The FinancialStatementGeneratorOptions that are configured on the Statement Generator WPF application
    /// </summary>
    public partial class FinancialStatementGeneratorOptions : FinancialStatementGeneratorOptionsEntity
    {
    }
}
