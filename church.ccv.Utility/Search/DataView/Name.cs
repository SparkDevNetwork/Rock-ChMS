﻿// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Search;

namespace church.ccv.Utility.Search.DataView
{
    /// <summary>
    /// Searches for DataViews with matching names
    /// </summary>
    [Description("DataView Search")]
    [Export(typeof(SearchComponent))]
    [ExportMetadata("ComponentName", "DataView")]
    public class Name : SearchComponent
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "SearchLabel", "DataView" );
                return defaults;
            }
        }

        /// <summary>
        /// Returns a list of matching items
        /// </summary>
        /// <param name="searchterm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchterm )
        {
            return new DataViewService( new RockContext() ).Queryable()
                .Where( g => 
                    g.Name.Contains( searchterm ) )
                .OrderBy( g => g.Name )
                .Select( g => g.Name );
        }
    }
}