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
    /// WorkflowTrigger View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.WorkflowTrigger ) )]
    public partial class WorkflowTriggerViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the EntityTypeId.
        /// </summary>
        /// <value>
        /// The EntityTypeId.
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeQualifierColumn.
        /// </summary>
        /// <value>
        /// The EntityTypeQualifierColumn.
        /// </value>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeQualifierValue.
        /// </summary>
        /// <value>
        /// The EntityTypeQualifierValue.
        /// </value>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeQualifierValuePrevious.
        /// </summary>
        /// <value>
        /// The EntityTypeQualifierValuePrevious.
        /// </value>
        public string EntityTypeQualifierValuePrevious { get; set; }

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
        /// Gets or sets the WorkflowName.
        /// </summary>
        /// <value>
        /// The WorkflowName.
        /// </value>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowTriggerType.
        /// </summary>
        /// <value>
        /// The WorkflowTriggerType.
        /// </value>
        public int WorkflowTriggerType { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowTypeId.
        /// </summary>
        /// <value>
        /// The WorkflowTypeId.
        /// </value>
        public int WorkflowTypeId { get; set; }

    }
}