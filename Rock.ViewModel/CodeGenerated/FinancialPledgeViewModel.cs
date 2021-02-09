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

namespace Rock.ViewModel
{
    /// <summary>
    /// FinancialPledge View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.FinancialPledge ) )]
    public partial class FinancialPledgeViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the AccountId.
        /// </summary>
        /// <value>
        /// The AccountId.
        /// </value>
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the EndDate.
        /// </summary>
        /// <value>
        /// The EndDate.
        /// </value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the GroupId.
        /// </summary>
        /// <value>
        /// The GroupId.
        /// </value>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId.
        /// </summary>
        /// <value>
        /// The PersonAliasId.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the PledgeFrequencyValueId.
        /// </summary>
        /// <value>
        /// The PledgeFrequencyValueId.
        /// </value>
        public int? PledgeFrequencyValueId { get; set; }

        /// <summary>
        /// Gets or sets the StartDate.
        /// </summary>
        /// <value>
        /// The StartDate.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the TotalAmount.
        /// </summary>
        /// <value>
        /// The TotalAmount.
        /// </value>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the CreatedDateTime.
        /// </summary>
        /// <value>
        /// The CreatedDateTime.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ModifiedDateTime.
        /// </summary>
        /// <value>
        /// The ModifiedDateTime.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the CreatedByPersonAliasId.
        /// </summary>
        /// <value>
        /// The CreatedByPersonAliasId.
        /// </value>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the ModifiedByPersonAliasId.
        /// </summary>
        /// <value>
        /// The ModifiedByPersonAliasId.
        /// </value>
        public int? ModifiedByPersonAliasId { get; set; }

    }
}