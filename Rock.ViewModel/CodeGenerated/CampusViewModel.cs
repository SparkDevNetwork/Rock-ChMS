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
    /// Campus View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.Campus ) )]
    public partial class CampusViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the CampusStatusValueId.
        /// </summary>
        /// <value>
        /// The CampusStatusValueId.
        /// </value>
        public int? CampusStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the CampusTypeValueId.
        /// </summary>
        /// <value>
        /// The CampusTypeValueId.
        /// </value>
        public int? CampusTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// The Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the IsActive.
        /// </summary>
        /// <value>
        /// The IsActive.
        /// </value>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the IsSystem.
        /// </summary>
        /// <value>
        /// The IsSystem.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the LeaderPersonAliasId.
        /// </summary>
        /// <value>
        /// The LeaderPersonAliasId.
        /// </value>
        public int? LeaderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the LocationId.
        /// </summary>
        /// <value>
        /// The LocationId.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// The Name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// The Order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the PhoneNumber.
        /// </summary>
        /// <value>
        /// The PhoneNumber.
        /// </value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the ServiceTimes.
        /// </summary>
        /// <value>
        /// The ServiceTimes.
        /// </value>
        public string ServiceTimes { get; set; }

        /// <summary>
        /// Gets or sets the ShortCode.
        /// </summary>
        /// <value>
        /// The ShortCode.
        /// </value>
        public string ShortCode { get; set; }

        /// <summary>
        /// Gets or sets the TeamGroupId.
        /// </summary>
        /// <value>
        /// The TeamGroupId.
        /// </value>
        public int? TeamGroupId { get; set; }

        /// <summary>
        /// Gets or sets the TimeZoneId.
        /// </summary>
        /// <value>
        /// The TimeZoneId.
        /// </value>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the Url.
        /// </summary>
        /// <value>
        /// The Url.
        /// </value>
        public string Url { get; set; }

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