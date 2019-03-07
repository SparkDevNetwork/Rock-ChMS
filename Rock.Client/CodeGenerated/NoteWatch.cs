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
    /// Base client model for NoteWatch that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class NoteWatchEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public bool AllowOverride { get; set; } = true;

        /// <summary />
        public int? EntityId { get; set; }

        /// <summary />
        public int? EntityTypeId { get; set; }

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public bool IsWatching { get; set; } = true;

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public int? NoteId { get; set; }

        /// <summary />
        public int? NoteTypeId { get; set; }

        /// <summary />
        public int? WatcherGroupId { get; set; }

        /// <summary />
        public int? WatcherPersonAliasId { get; set; }

        /// <summary />
        public bool WatchReplies { get; set; }

        /// <summary />
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
        /// Copies the base properties from a source NoteWatch object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( NoteWatch source )
        {
            this.Id = source.Id;
            this.AllowOverride = source.AllowOverride;
            this.EntityId = source.EntityId;
            this.EntityTypeId = source.EntityTypeId;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.IsWatching = source.IsWatching;
            this.NoteId = source.NoteId;
            this.NoteTypeId = source.NoteTypeId;
            this.WatcherGroupId = source.WatcherGroupId;
            this.WatcherPersonAliasId = source.WatcherPersonAliasId;
            this.WatchReplies = source.WatchReplies;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for NoteWatch that includes all the fields that are available for GETs. Use this for GETs (use NoteWatchEntity for POST/PUTs)
    /// </summary>
    public partial class NoteWatch : NoteWatchEntity
    {
        /// <summary />
        public EntityType EntityType { get; set; }

        /// <summary />
        public Note Note { get; set; }

        /// <summary />
        public NoteType NoteType { get; set; }

        /// <summary />
        public Group WatcherGroup { get; set; }

        /// <summary />
        public PersonAlias WatcherPersonAlias { get; set; }

    }
}
