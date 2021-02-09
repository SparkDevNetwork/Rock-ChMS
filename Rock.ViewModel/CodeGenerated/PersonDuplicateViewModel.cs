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
    /// PersonDuplicate View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.PersonDuplicate ) )]
    public partial class PersonDuplicateViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the Capacity.
        /// </summary>
        /// <value>
        /// The Capacity.
        /// </value>
        public int? Capacity { get; set; }

        /// <summary>
        /// Gets or sets the DuplicatePersonAliasId.
        /// </summary>
        /// <value>
        /// The DuplicatePersonAliasId.
        /// </value>
        public int DuplicatePersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the IgnoreUntilScoreChanges.
        /// </summary>
        /// <value>
        /// The IgnoreUntilScoreChanges.
        /// </value>
        public bool IgnoreUntilScoreChanges { get; set; }

        /// <summary>
        /// Gets or sets the IsConfirmedAsNotDuplicate.
        /// </summary>
        /// <value>
        /// The IsConfirmedAsNotDuplicate.
        /// </value>
        public bool IsConfirmedAsNotDuplicate { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId.
        /// </summary>
        /// <value>
        /// The PersonAliasId.
        /// </value>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Score.
        /// </summary>
        /// <value>
        /// The Score.
        /// </value>
        public int? Score { get; set; }

        /// <summary>
        /// Gets or sets the ScoreDetail.
        /// </summary>
        /// <value>
        /// The ScoreDetail.
        /// </value>
        public string ScoreDetail { get; set; }

        /// <summary>
        /// Gets or sets the TotalCapacity.
        /// </summary>
        /// <value>
        /// The TotalCapacity.
        /// </value>
        public int? TotalCapacity { get; set; }

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