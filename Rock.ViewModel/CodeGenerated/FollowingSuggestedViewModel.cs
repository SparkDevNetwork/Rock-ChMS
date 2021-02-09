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
    /// FollowingSuggested View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.FollowingSuggested ) )]
    public partial class FollowingSuggestedViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the EntityId.
        /// </summary>
        /// <value>
        /// The EntityId.
        /// </value>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId.
        /// </summary>
        /// <value>
        /// The EntityTypeId.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the LastPromotedDateTime.
        /// </summary>
        /// <value>
        /// The LastPromotedDateTime.
        /// </value>
        public DateTime? LastPromotedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId.
        /// </summary>
        /// <value>
        /// The PersonAliasId.
        /// </value>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        /// <value>
        /// The Status.
        /// </value>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the StatusChangedDateTime.
        /// </summary>
        /// <value>
        /// The StatusChangedDateTime.
        /// </value>
        public DateTime StatusChangedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the SuggestionTypeId.
        /// </summary>
        /// <value>
        /// The SuggestionTypeId.
        /// </value>
        public int SuggestionTypeId { get; set; }

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