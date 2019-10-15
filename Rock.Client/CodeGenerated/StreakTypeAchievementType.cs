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
    /// Base client model for StreakTypeAchievementType that only includes the non-virtual fields. Use this for PUT/POSTs
    /// </summary>
    public partial class StreakTypeAchievementTypeEntity
    {
        /// <summary />
        public int Id { get; set; }

        /// <summary />
        public int? AchievementEndWorkflowTypeId { get; set; }

        /// <summary />
        public int AchievementEntityTypeId { get; set; }

        /// <summary />
        public string AchievementIconCssClass { get; set; }

        /// <summary />
        public int? AchievementStartWorkflowTypeId { get; set; }

        /// <summary />
        public int? AchievementStepStatusId { get; set; }

        /// <summary />
        public int? AchievementStepTypeId { get; set; }

        /// <summary />
        public bool AllowOverAchievement { get; set; }

        /// <summary />
        public string BadgeLavaTemplate { get; set; }

        /// <summary />
        public int? CategoryId { get; set; }

        /// <summary />
        public string Description { get; set; }

        /// <summary />
        public Guid? ForeignGuid { get; set; }

        /// <summary />
        public string ForeignKey { get; set; }

        /// <summary />
        public bool IsActive { get; set; }

        /// <summary />
        public int? MaxAccomplishmentsAllowed { get; set; } = 1;

        /// <summary>
        /// If the ModifiedByPersonAliasId is being set manually and should not be overwritten with current user when saved, set this value to true
        /// </summary>
        public bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary />
        public string Name { get; set; }

        /// <summary />
        public string ResultsLavaTemplate { get; set; }

        /// <summary />
        public int StreakTypeId { get; set; }

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
        /// Copies the base properties from a source StreakTypeAchievementType object
        /// </summary>
        /// <param name="source">The source.</param>
        public void CopyPropertiesFrom( StreakTypeAchievementType source )
        {
            this.Id = source.Id;
            this.AchievementEndWorkflowTypeId = source.AchievementEndWorkflowTypeId;
            this.AchievementEntityTypeId = source.AchievementEntityTypeId;
            this.AchievementIconCssClass = source.AchievementIconCssClass;
            this.AchievementStartWorkflowTypeId = source.AchievementStartWorkflowTypeId;
            this.AchievementStepStatusId = source.AchievementStepStatusId;
            this.AchievementStepTypeId = source.AchievementStepTypeId;
            this.AllowOverAchievement = source.AllowOverAchievement;
            this.BadgeLavaTemplate = source.BadgeLavaTemplate;
            this.CategoryId = source.CategoryId;
            this.Description = source.Description;
            this.ForeignGuid = source.ForeignGuid;
            this.ForeignKey = source.ForeignKey;
            this.IsActive = source.IsActive;
            this.MaxAccomplishmentsAllowed = source.MaxAccomplishmentsAllowed;
            this.ModifiedAuditValuesAlreadyUpdated = source.ModifiedAuditValuesAlreadyUpdated;
            this.Name = source.Name;
            this.ResultsLavaTemplate = source.ResultsLavaTemplate;
            this.StreakTypeId = source.StreakTypeId;
            this.CreatedDateTime = source.CreatedDateTime;
            this.ModifiedDateTime = source.ModifiedDateTime;
            this.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            this.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            this.Guid = source.Guid;
            this.ForeignId = source.ForeignId;

        }
    }

    /// <summary>
    /// Client model for StreakTypeAchievementType that includes all the fields that are available for GETs. Use this for GETs (use StreakTypeAchievementTypeEntity for POST/PUTs)
    /// </summary>
    public partial class StreakTypeAchievementType : StreakTypeAchievementTypeEntity
    {
        /// <summary />
        public WorkflowType AchievementEndWorkflowType { get; set; }

        /// <summary />
        public EntityType AchievementEntityType { get; set; }

        /// <summary />
        public WorkflowType AchievementStartWorkflowType { get; set; }

        /// <summary />
        public StepStatus AchievementStepStatus { get; set; }

        /// <summary />
        public StepType AchievementStepType { get; set; }

        /// <summary />
        public Category Category { get; set; }

        /// <summary />
        public ICollection<StreakAchievementAttempt> StreakAchievementAttempts { get; set; }

        /// <summary />
        public StreakType StreakType { get; set; }

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