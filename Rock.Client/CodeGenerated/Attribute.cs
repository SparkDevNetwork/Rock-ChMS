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
    /// Base client model for Attribute that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class AttributeEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public bool AllowSearch { get; set; }

        /// <summary />
        public string DefaultValue { get; set; }

        /// <summary />
        public string Description { get; set; }

        /// <summary />
        public bool EnableHistory { get; set; }

        /// <summary />
        public int? EntityTypeId { get; set; }

        /// <summary />
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary />
        public string EntityTypeQualifierValue { get; set; }

        /// <summary />
        public int FieldTypeId { get; set; }

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public string IconCssClass { get; set; }

        /// <summary />
        public bool IsActive { get; set; } = true;

        /// <summary />
        public bool IsAnalytic { get; set; }

        /// <summary />
        public bool IsAnalyticHistory { get; set; }

        /// <summary />
        public bool IsGridColumn { get; set; }

        /// <summary />
        public bool IsIndexEnabled { get; set; }

        /// <summary />
        public bool IsMultiValue { get; set; }

        /// <summary />
        public bool IsRequired { get; set; }

        /// <summary />
        public bool IsSystem { get; set; }

        /// <summary />
        public string Key { get; set; }

        /// <summary />
        public string Name { get; set; }

        /// <summary />
        public int Order { get; set; }

        /// <summary />
        public string PostHtml { get; set; }

        /// <summary />
        public string PreHtml { get; set; }

        /// <summary>
        /// Leave this as NULL to let Rock set this
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary />
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary />
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary />
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary />
        public Guid Guid { get; set; }

        /// <summary />
        public int? ForeignId { get; set; }

        /// <summary>
        /// Copies the base properties from a source Attribute object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( Attribute source )
        {
            this.Id = source.Id;
            this.AllowSearch = source.AllowSearch;
            this.DefaultValue = source.DefaultValue;
            this.Description = source.Description;
            this.EnableHistory = source.EnableHistory;
            this.EntityTypeId = source.EntityTypeId;
            this.EntityTypeQualifierColumn = source.EntityTypeQualifierColumn;
            this.EntityTypeQualifierValue = source.EntityTypeQualifierValue;
            this.FieldTypeId = source.FieldTypeId;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.IconCssClass = source.IconCssClass;
            this.IsActive = source.IsActive;
            this.IsAnalytic = source.IsAnalytic;
            this.IsAnalyticHistory = source.IsAnalyticHistory;
            this.IsGridColumn = source.IsGridColumn;
            this.IsIndexEnabled = source.IsIndexEnabled;
            this.IsMultiValue = source.IsMultiValue;
            this.IsRequired = source.IsRequired;
            this.IsSystem = source.IsSystem;
            this.Key = source.Key;
            this.Name = source.Name;
            this.Order = source.Order;
            this.PostHtml = source.PostHtml;
            this.PreHtml = source.PreHtml;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for Attribute that includes all the fields that are available for GETs. Use this for GETs (use AttributeEntity for POST/PUTs)
    /// </summary>
    public partial class Attribute : AttributeEntity
    {
        /// <summary />
        public ICollection<AttributeQualifier> AttributeQualifiers { get; set; }

        /// <summary />
        public ICollection<Category> Categories { get; set; }

        /// <summary />
        public EntityType EntityType { get; set; }

        /// <summary />
        public FieldType FieldType { get; set; }

    }
}
