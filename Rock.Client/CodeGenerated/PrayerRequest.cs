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
    /// Base client model for PrayerRequest that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class PrayerRequestEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public bool? AllowComments { get; set; }

        /// <summary />
        public string Answer { get; set; }

        /// <summary />
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary />
        public DateTime? ApprovedOnDateTime { get; set; }

        /// <summary />
        public int? CampusId { get; set; }

        /// <summary />
        public int? CategoryId { get; set; }

        /// <summary />
        public string Email { get; set; }

        /// <summary />
        public DateTime EnteredDateTime { get; set; }

        /// <summary />
        public DateTime? ExpirationDate { get; set; }

        /// <summary />
        public string FirstName { get; set; }

        /// <summary />
        public int? FlagCount { get; set; }

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public int? GroupId { get; set; }

        /// <summary />
        public bool? IsActive { get; set; }

        /// <summary />
        public bool? IsApproved { get; set; }

        /// <summary />
        public bool? IsPublic { get; set; }

        /// <summary />
        public bool? IsUrgent { get; set; }

        /// <summary />
        public string LastName { get; set; }

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public int? PrayerCount { get; set; }

        /// <summary />
        public int? RequestedByPersonAliasId { get; set; }

        /// <summary />
        public string Text { get; set; }

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
        /// Copies the base properties from a source PrayerRequest object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( PrayerRequest source )
        {
            this.Id = source.Id;
            this.AllowComments = source.AllowComments;
            this.Answer = source.Answer;
            this.ApprovedByPersonAliasId = source.ApprovedByPersonAliasId;
            this.ApprovedOnDateTime = source.ApprovedOnDateTime;
            this.CampusId = source.CampusId;
            this.CategoryId = source.CategoryId;
            this.Email = source.Email;
            this.EnteredDateTime = source.EnteredDateTime;
            this.ExpirationDate = source.ExpirationDate;
            this.FirstName = source.FirstName;
            this.FlagCount = source.FlagCount;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.GroupId = source.GroupId;
            this.IsActive = source.IsActive;
            this.IsApproved = source.IsApproved;
            this.IsPublic = source.IsPublic;
            this.IsUrgent = source.IsUrgent;
            this.LastName = source.LastName;
            this.ModifiedAuditValuesAlreadyUpdated = source.ModifiedAuditValuesAlreadyUpdated;
            this.PrayerCount = source.PrayerCount;
            this.RequestedByPersonAliasId = source.RequestedByPersonAliasId;
            this.Text = source.Text;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for PrayerRequest that includes all the fields that are available for GETs. Use this for GETs (use PrayerRequestEntity for POST/PUTs)
    /// </summary>
    public partial class PrayerRequest : PrayerRequestEntity
    {
        /// <summary />
        public PersonAlias ApprovedByPersonAlias { get; set; }

        /// <summary />
        public Category Category { get; set; }

        /// <summary />
        public PersonAlias RequestedByPersonAlias { get; set; }

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
