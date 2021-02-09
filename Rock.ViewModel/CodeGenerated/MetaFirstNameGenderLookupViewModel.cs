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
    /// MetaFirstNameGenderLookup View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.MetaFirstNameGenderLookup ) )]
    public partial class MetaFirstNameGenderLookupViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the Country.
        /// </summary>
        /// <value>
        /// The Country.
        /// </value>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the FemaleCount.
        /// </summary>
        /// <value>
        /// The FemaleCount.
        /// </value>
        public int? FemaleCount { get; set; }

        /// <summary>
        /// Gets or sets the FemalePercent.
        /// </summary>
        /// <value>
        /// The FemalePercent.
        /// </value>
        public decimal? FemalePercent { get; set; }

        /// <summary>
        /// Gets or sets the FirstName.
        /// </summary>
        /// <value>
        /// The FirstName.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the Language.
        /// </summary>
        /// <value>
        /// The Language.
        /// </value>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the MaleCount.
        /// </summary>
        /// <value>
        /// The MaleCount.
        /// </value>
        public int? MaleCount { get; set; }

        /// <summary>
        /// Gets or sets the MalePercent.
        /// </summary>
        /// <value>
        /// The MalePercent.
        /// </value>
        public decimal? MalePercent { get; set; }

        /// <summary>
        /// Gets or sets the TotalCount.
        /// </summary>
        /// <value>
        /// The TotalCount.
        /// </value>
        public int? TotalCount { get; set; }

    }
}