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
    /// ConnectionOpportunityGroupConfig View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.ConnectionOpportunityGroupConfig ) )]
    public partial class ConnectionOpportunityGroupConfigViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the ConnectionOpportunityId.
        /// </summary>
        /// <value>
        /// The ConnectionOpportunityId.
        /// </value>
        public int ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the GroupMemberRoleId.
        /// </summary>
        /// <value>
        /// The GroupMemberRoleId.
        /// </value>
        public int? GroupMemberRoleId { get; set; }

        /// <summary>
        /// Gets or sets the GroupMemberStatus.
        /// </summary>
        /// <value>
        /// The GroupMemberStatus.
        /// </value>
        public int GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the GroupTypeId.
        /// </summary>
        /// <value>
        /// The GroupTypeId.
        /// </value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the UseAllGroupsOfType.
        /// </summary>
        /// <value>
        /// The UseAllGroupsOfType.
        /// </value>
        public bool UseAllGroupsOfType { get; set; }

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