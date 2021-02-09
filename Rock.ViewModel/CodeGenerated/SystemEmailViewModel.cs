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
    /// SystemEmail View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.SystemEmail ) )]
    public partial class SystemEmailViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the Bcc.
        /// </summary>
        /// <value>
        /// The Bcc.
        /// </value>
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the Body.
        /// </summary>
        /// <value>
        /// The Body.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId.
        /// </summary>
        /// <value>
        /// The CategoryId.
        /// </value>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the Cc.
        /// </summary>
        /// <value>
        /// The Cc.
        /// </value>
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets the From.
        /// </summary>
        /// <value>
        /// The From.
        /// </value>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets the FromName.
        /// </summary>
        /// <value>
        /// The FromName.
        /// </value>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the IsSystem.
        /// </summary>
        /// <value>
        /// The IsSystem.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Subject.
        /// </summary>
        /// <value>
        /// The Subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// The Title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the To.
        /// </summary>
        /// <value>
        /// The To.
        /// </value>
        public string To { get; set; }

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