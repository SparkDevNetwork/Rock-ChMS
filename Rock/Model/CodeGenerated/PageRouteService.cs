//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// PageRoute Service class
    /// </summary>
    public partial class PageRouteService : Service<PageRoute>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageRouteService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public PageRouteService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( PageRoute item, out string errorMessage )
        {
            errorMessage = string.Empty;
 
            if ( new Service<Site>( Context ).Queryable().Any( a => a.ChangePasswordPageRouteId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", PageRoute.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<Site>( Context ).Queryable().Any( a => a.CommunicationPageRouteId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", PageRoute.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<Site>( Context ).Queryable().Any( a => a.DefaultPageRouteId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", PageRoute.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<Site>( Context ).Queryable().Any( a => a.LoginPageRouteId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", PageRoute.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<Site>( Context ).Queryable().Any( a => a.PageNotFoundPageRouteId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", PageRoute.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }  
 
            if ( new Service<Site>( Context ).Queryable().Any( a => a.RegistrationPageRouteId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", PageRoute.FriendlyTypeName, Site.FriendlyTypeName );
                return false;
            }  
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class PageRouteExtensionMethods
    {
        /// <summary>
        /// Clones this PageRoute object to a new PageRoute object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static PageRoute Clone( this PageRoute source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as PageRoute;
            }
            else
            {
                var target = new PageRoute();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another PageRoute object to this PageRoute object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this PageRoute target, PageRoute source )
        {
            target.Id = source.Id;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.IsSystem = source.IsSystem;
            target.PageId = source.PageId;
            target.Route = source.Route;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
