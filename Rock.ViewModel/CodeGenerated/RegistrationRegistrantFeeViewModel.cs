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
    /// RegistrationRegistrantFee View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.RegistrationRegistrantFee ) )]
    public partial class RegistrationRegistrantFeeViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the Cost.
        /// </summary>
        /// <value>
        /// The Cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the Option.
        /// </summary>
        /// <value>
        /// The Option.
        /// </value>
        public string Option { get; set; }

        /// <summary>
        /// Gets or sets the Quantity.
        /// </summary>
        /// <value>
        /// The Quantity.
        /// </value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the RegistrationRegistrantId.
        /// </summary>
        /// <value>
        /// The RegistrationRegistrantId.
        /// </value>
        public int RegistrationRegistrantId { get; set; }

        /// <summary>
        /// Gets or sets the RegistrationTemplateFeeId.
        /// </summary>
        /// <value>
        /// The RegistrationTemplateFeeId.
        /// </value>
        public int RegistrationTemplateFeeId { get; set; }

        /// <summary>
        /// Gets or sets the RegistrationTemplateFeeItemId.
        /// </summary>
        /// <value>
        /// The RegistrationTemplateFeeItemId.
        /// </value>
        public int? RegistrationTemplateFeeItemId { get; set; }

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