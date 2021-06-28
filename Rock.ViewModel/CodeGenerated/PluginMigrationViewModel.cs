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
using System.Linq;

namespace Rock.ViewModel
{
    /// <summary>
    /// PluginMigration View Model
    /// </summary>
    public partial class PluginMigrationViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the MigrationName.
        /// </summary>
        /// <value>
        /// The MigrationName.
        /// </value>
        public string MigrationName { get; set; }

        /// <summary>
        /// Gets or sets the MigrationNumber.
        /// </summary>
        /// <value>
        /// The MigrationNumber.
        /// </value>
        public int MigrationNumber { get; set; }

        /// <summary>
        /// Gets or sets the PluginAssemblyName.
        /// </summary>
        /// <value>
        /// The PluginAssemblyName.
        /// </value>
        public string PluginAssemblyName { get; set; }

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
