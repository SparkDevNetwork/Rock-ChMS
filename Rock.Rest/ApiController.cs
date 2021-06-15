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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData;

#if NET5_0_OR_GREATER
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FromUriAttribute = Microsoft.AspNetCore.Mvc.FromQueryAttribute;
#else
using IActionResult = System.Web.Http.IHttpActionResult;
#endif

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Tasks;
using Rock.Web.Cache;
using System.Threading.Tasks;
using System.Threading;

namespace Rock.Rest
{
    /// <summary>
    /// ApiController for Rock REST Entity endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ApiController<T> : ApiControllerBase
        where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        protected Service<T> Service
        {
            get { return _service; }
            set { _service = value; }
        }

        private Service<T> _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiController{T}"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public ApiController( Service<T> service )
        {
            Service = service;

            // Turn off proxy creation by default so that when querying objects through rest, EF does not automatically navigate all child properties for requested objects
            // When adding, updating, or deleting objects, the proxy should be enabled to properly track relationships that should or shouldn't be updated
            SetProxyCreation( false );
        }

        /// <summary>
        /// Queryable GET endpoint
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        public virtual IQueryable<T> Get()
        {
            var result = Service.Queryable().AsNoTracking();

            return Ok(result);
        }

        /// <summary>
        /// GET endpoint to get a single record 
        /// </summary>
        /// <param name="id">The Id of the record</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public virtual T GetById( int id )
        {
            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                return NotFound();
            }

            return Ok( model );
        }

        /// <summary>
        /// GET endpoint to get a single record 
        /// </summary>
        /// <param name="key">The Id of the record</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [EnableQuery]
        public virtual T Get( [FromODataUri] int key )
        {
            T model;
            if ( !Service.TryGet( key, out model ) )
            {
                NotFound();
            }

            return Ok( model );
        }

        /// <summary>
        /// Gets records that have a particular attribute value.
        /// Example: api/People/GetByAttributeValue?attributeKey=FirstVisit&amp;value=2012-12-15
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [ActionName( "GetByAttributeValue" )]
        [EnableQuery]
        public virtual IQueryable<T> GetByAttributeValue( [FromUri] int? attributeId = null, [FromUri] string attributeKey = null, [FromUri] string value = null, [FromUri] bool caseSensitive = false )
        {
            // Value is always required
            if ( value.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The value param is required" );
                throw new HttpResponseException( errorResponse );
            }

            // Either key or id is required, but not both
            var queryByKey = !attributeKey.IsNullOrWhiteSpace();
            var queryById = attributeId.HasValue;

            if ( queryByKey == queryById )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "Either attributeKey or attributeId must be specified, but not both" );
                throw new HttpResponseException( errorResponse );
            }

            // Query for the models that have the value for the attribute
            var rockContext = Service.Context as RockContext;
            var query = Service.Queryable().AsNoTracking();
            var valueComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if ( queryById )
            {
                query = query.WhereAttributeValue( rockContext,
                    a => a.AttributeId == attributeId && a.Value.Equals( value, valueComparison ) );
            }
            else
            {
                query = query.WhereAttributeValue( rockContext,
                    a => a.Attribute.Key.Equals( attributeKey, StringComparison.OrdinalIgnoreCase ) && a.Value.Equals( value, valueComparison ) );
            }

            return Ok( query );
        }

        /// <summary>
        /// Gets items associated with a campus using the EntityCampusFilter model. The Entity must implement ICampusFilterable.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [ActionName( "GetByCampus" )]
        [EnableQuery]
        public virtual IQueryable<T> GetByCampus( [FromUri] int campusId )
        {
            if ( !typeof( T ).GetInterfaces().Contains( typeof( ICampusFilterable ) ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The model does not support campus filtering." );
                throw new HttpResponseException( errorResponse );
            }

            var rockContext = Service.Context as RockContext;
            var result = Service
                .Queryable()
                .AsNoTracking()
                .WhereCampus( rockContext, campusId );

            return Ok( result );
        }

        /// <summary>
        /// POST endpoint. Use this to INSERT a new record
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        public virtual HttpResponseMessage Post( [FromBody] T value )
        {
            if ( value == null )
            {
                return BadRequest();
            }

            SetProxyCreation( true );

            if ( !TryCheckCanEdit( value, out var checkResult ) )
            {
                return checkResult;
            }

            Service.Add( value );

            if ( !value.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", value.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            EnsureHttpContextHasCurrentPerson();

            Service.Context.SaveChanges();

            //// TODO set response.Headers.Location as per REST POST convention
            // response.Headers.Location = new Uri( Request.RequestUri, "/api/pages/" + page.Id.ToString() );
#if NET5_0_OR_GREATER
            return StatusCode( StatusCodes.Status201Created, value.Id );
#else
            return Content( HttpStatusCode.Created, value.Id );
#endif
        }

        /// <summary>
        /// PUT endpoint. Use this to UPDATE a record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        public virtual void Put( int id, [FromBody] T value )
        {
            if ( value == null )
            {
                return BadRequest();
            }

            SetProxyCreation( true );

            T targetModel;
            if ( !Service.TryGet( id, out targetModel ) )
            {
                return NotFound();
            }

            if ( !TryCheckCanEdit( targetModel, out var checkResult ) )
            {
                return checkResult;
            }

            Service.SetValues( value, targetModel );

            if ( targetModel.IsValid )
            {
                EnsureHttpContextHasCurrentPerson();

                Service.Context.SaveChanges();

                return NoContent();
            }
            else
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", targetModel.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                throw new HttpResponseException( response );
            }
        }

        /// <summary>
        /// PATCH endpoint. Use this to update a subset of the properties of the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        public virtual void Patch( int id, [FromBody] Dictionary<string, object> values )
        {
            // Check that something was sent in the body
            if ( values == null || !values.Keys.Any() )
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, "No values were sent in the body" );
                throw new HttpResponseException( response );
            }
            else if ( values.ContainsKey( "Id" ) )
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, "Cannot set Id" );
                throw new HttpResponseException( response );
            }

            SetProxyCreation( true );

            T targetModel;
            if ( !Service.TryGet( id, out targetModel ) )
            {
                return NotFound();
            }

            if ( !TryCheckCanEdit( targetModel, out var checkResult ) )
            {
                return checkResult;
            }

            var type = targetModel.GetType();
            var properties = type.GetProperties().ToList();

            // Same functionality as Service.SetValues but for a subset of properties
            foreach ( var key in values.Keys )
            {
                if ( properties.Any( p => p.Name.Equals( key ) ) )
                {
                    var property = type.GetProperty( key, BindingFlags.Public | BindingFlags.Instance );
                    var propertyType = Nullable.GetUnderlyingType( property.PropertyType ) ?? property.PropertyType;
                    var currentValue = values[key];

                    if ( property != null )
                    {
                        if ( property.GetValue( targetModel ) == currentValue )
                        {
                            continue;
                        }
                        else if ( property.CanWrite )
                        {
                            if ( currentValue == null )
                            {
                                // No need to parse anything
                                property.SetValue( targetModel, null );
                            }
                            else if ( propertyType == typeof( int ) || propertyType == typeof( int? ) || propertyType.IsEnum )
                            {
                                // By default, objects that hold integer values, hold int64, so coerce to int32
                                try
                                {
                                    var int32 = Convert.ToInt32( currentValue );
                                    property.SetValue( targetModel, int32 );
                                }
                                catch ( OverflowException )
                                {
                                    var response = ControllerContext.Request.CreateErrorResponse(
                                        HttpStatusCode.BadRequest,
                                        string.Format( "Cannot cast {0} to int32", key ) );
                                    throw new HttpResponseException( response );
                                }
                            }
                            else
                            {
                                var castedValue = Convert.ChangeType( currentValue, propertyType );
                                property.SetValue( targetModel, castedValue );
                            }
                        }
                        else
                        {
                            var response = ControllerContext.Request.CreateErrorResponse(
                                HttpStatusCode.BadRequest,
                                string.Format( "Cannot write {0}", key ) );
                            throw new HttpResponseException( response );
                        }
                    }
                    else
                    {
                        // This shouldn't happen because we are checking that the property exists.
                        // Just to make sure reflection doesn't fail
                        var response = ControllerContext.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            string.Format( "Cannot find property {0}", key ) );
                        throw new HttpResponseException( response );
                    }
                }
                else
                {
                    var response = ControllerContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format( "{0} does not have attribute {1}", type.BaseType.Name, key ) );
                    throw new HttpResponseException( response );
                }
            }

            // Verify model is valid before saving
            if ( targetModel.IsValid )
            {
                EnsureHttpContextHasCurrentPerson();

                Service.Context.SaveChanges();

                return NoContent();
            }
            else
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", targetModel.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                throw new HttpResponseException( response );
            }
        }

        /// <summary>
        /// DELETE endpoint. To delete the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        public virtual void Delete( int id )
        {
            SetProxyCreation( true );

            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                return NotFound();
            }

            if ( !TryCheckCanEdit( model, out var checkResult ) )
            {
                return checkResult;
            }

            Service.Delete( model );
            Service.Context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// Gets a list of objects represented by the selected data view
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        [EnableQuery]
        public IQueryable<T> GetDataView( int id )
        {
            var rockContext = new RockContext();
            var dataView = new DataViewService( rockContext ).Get( id );

            if ( !TryValidateDataView( dataView, out var validateResult ) )
            {
                return validateResult;
            }

            var paramExpression = Service.ParameterExpression;
            var whereExpression = dataView.GetExpression( Service, paramExpression );

            return Ok( Service.GetNoTracking( paramExpression, whereExpression ) );
        }

        /// <summary>
        /// Determines if the entity id is in the data view
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "InDataView" )]
        [EnableQuery]
        [HttpGet]
        public bool InDataView( int dataViewId, int entityId )
        {
            var rockContext = new RockContext();

            var dataView = new DataViewService( rockContext ).Get( dataViewId );

            if ( !TryValidateDataView( dataView, out var validateResult ) )
            {
                return validateResult;
            }

            var dataViewGetQueryArgs = new DataViewGetQueryArgs
            {
                DbContext = rockContext
            };

            var qryGroupsInDataView = dataView.GetQuery( dataViewGetQueryArgs ) as IQueryable<T>;
            qryGroupsInDataView = qryGroupsInDataView.Where( d => d.Id == entityId );

            return Ok( qryGroupsInDataView.Any() );
        }

        /// <summary>
        /// Checks to makes sure DataView exists, has the correct entityType, and person has View rights, etc
        /// </summary>
        /// <param name="dataView">The data view.</param>
        /// <param name="result">On return contains the result that should be returned if the method returns false.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        private void ValidateDataView( DataView dataView )
        {
            if ( dataView == null )
            {
                result = NotFound();

                return false;
            }

            var controllerEntityType = EntityTypeCache.Get<T>();
            if ( dataView.EntityTypeId != controllerEntityType?.Id )
            {
                result = BadRequest( $"{dataView.Name} is not a {controllerEntityType?.Name} dataview" );

                return false;
            }

            // since DataViews can be secured at the DataView or Category level, specifically check for CanView
            return TryCheckCanView( dataView, GetPerson(), out result );
        }

        /// <summary>
        /// Launches a workflow. And optionally passes the entity with selected id as the entity for the workflow
        /// </summary>
        /// <param name="id">The Id of the entity to pass to workflow, if entity cannot be loaded workflow will still be launched but without passing an entity</param>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowName">The Name of the workflow.</param>
        /// <param name="workflowAttributeValues">Optional list of workflow values to set.</param>
        [Authenticate, Secured]
        [ActionName( "LaunchWorkflow" )]
        [HttpPost]
        public void LaunchWorkflow( int id, Guid workflowTypeGuid, string workflowName, [FromBody] Dictionary<string, string> workflowAttributeValues )
        {
            T entity = null;
            if ( id > 0 )
            {
                entity = Get( id );
            }

            if ( entity != null )
            {
                entity.LaunchWorkflow( workflowTypeGuid, workflowName, workflowAttributeValues );
            }
            else
            {
                new LaunchWorkflow.Message
                {
                    WorkflowTypeGuid = workflowTypeGuid,
                    WorkflowName = workflowName,
                    WorkflowAttributeValues = workflowAttributeValues
                }.Send();
            }
        }

        /// <summary>
        /// Launches a workflow. And optionally passes the entity with selected id as the entity for the workflow
        /// </summary>
        /// <param name="id">The Id of the entity to pass to workflow, if entity cannot be loaded workflow will still be launched but without passing an entity</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="workflowAttributeValues">Optional list of workflow values to set.</param>
        [Authenticate, Secured]
        [ActionName( "LaunchWorkflow" )]
        [HttpPost]
        public void LaunchWorkflow( int id, int workflowTypeId, string workflowName, [FromBody] Dictionary<string, string> workflowAttributeValues )
        {
            T entity = null;
            if ( id > 0 )
            {
                entity = Get( id );
            }

            if ( entity != null )
            {
                entity.LaunchWorkflow( workflowTypeId, workflowName, workflowAttributeValues );
            }
            else
            {
                new LaunchWorkflow.Message
                {
                    WorkflowTypeId = workflowTypeId,
                    WorkflowName = workflowName,
                    WorkflowAttributeValues = workflowAttributeValues
                }.Send();
            }
        }

        /// <summary>
        /// Gets a query of the items that are followed by a specific person. For example, ~/api/Groups/FollowedItems
        /// would return a list of groups that the person is following. Either ?personId= or ?personAliasId= can be
        /// specified to indicate what person you want to see the followed items for.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "FollowedItems" )]
        [EnableQuery]
        public IQueryable<T> GetFollowedItems( int? personId = null, int? personAliasId = null )
        {
            if ( !personId.HasValue )
            {
                if ( personAliasId.HasValue )
                {
                    personId = new PersonAliasService( this.Service.Context as RockContext ).GetPersonId( personAliasId.Value );
                }
            }

            if ( personId.HasValue )
            {
                return Ok( Service.GetFollowed( personId.Value ) );
            }

            throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.BadRequest ) { ReasonPhrase = "either personId or personAliasId must be specified" } );
        }

        /// <summary>
        /// DELETE to delete the specified attribute value for the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpDelete]
        public virtual HttpResponseMessage DeleteAttributeValue( int id, string attributeKey )
        {
            return SetAttributeValue( id, attributeKey, string.Empty );
        }

        /// <summary>
        /// POST an attribute value. Use this to set an attribute value for the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        /// <exception cref="HttpResponseMessage">
        /// </exception>
        [Authenticate, Secured]
        [HttpPost]
        public virtual HttpResponseMessage SetAttributeValue( int id, string attributeKey, string attributeValue )
        {
            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                return NotFound();
            }

            if ( !TryCheckCanEdit( model, out var checkResult ) )
            {
                return checkResult;
            }

            IHasAttributes modelWithAttributes = model as IHasAttributes;
            if ( modelWithAttributes != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    modelWithAttributes.LoadAttributes( rockContext );
                    Rock.Web.Cache.AttributeCache attributeCache = modelWithAttributes.Attributes.ContainsKey( attributeKey ) ? modelWithAttributes.Attributes[attributeKey] : null;

                    if ( attributeCache != null )
                    {
                        if ( !attributeCache.IsAuthorized( Rock.Security.Authorization.EDIT, this.GetPerson() ) )
                        {
                            throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.Forbidden ) { ReasonPhrase = string.Format( "Not authorized to edit {0} on {1}", modelWithAttributes.GetType().GetFriendlyTypeName(), attributeKey ) } );
                        }

                        Rock.Attribute.Helper.SaveAttributeValue( modelWithAttributes, attributeCache, attributeValue, rockContext );

                        return Accepted( modelWithAttributes.Id );
                    }
                    else
                    {
                        throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.BadRequest ) { ReasonPhrase = string.Format( "{0} does not have a {1} attribute", modelWithAttributes.GetType().GetFriendlyTypeName(), attributeKey ) } );
                    }
                }
            }
            else
            {
                throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.BadRequest ) { ReasonPhrase = "specified item does not have attributes" } );
            }
        }

        /// <summary>
        /// Sets the Context Cookie to the specified record. Use this to set the Campus Context, Group Context, etc
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpPut, HttpOptions]
        [ActionName( "SetContext" )]
        public virtual HttpResponseMessage SetContext( int id )
        {
            Guid? guid = Service.GetGuid( id );
            if ( !guid.HasValue )
            {
                return NotFound();
            }

            string cookieName = "Rock_Context";
            string typeName = typeof( T ).FullName;

            string identifier =
                typeName + "|" +
                id.ToString() + ">" +
                guid.ToString();
            string contextValue = Rock.Security.Encryption.EncryptString( identifier );

#if NET5_0_OR_GREATER
            var httpContext = HttpContext;
#else
            var httpContext = System.Web.HttpContext.Current;
#endif
            if ( httpContext == null )
            {
                return BadRequest();
            }

#if NET5_0_OR_GREATER
            httpContext.Response.Cookies.Append( cookieName, contextValue, new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = RockDateTime.Now.AddYears( 1 )
            } );
#else
            var contextCookie = httpContext.Request.Cookies[cookieName] ?? new System.Web.HttpCookie( cookieName );
            contextCookie.Values[typeName] = contextValue;
            contextCookie.Expires = RockDateTime.Now.AddYears( 1 );
            Rock.Web.UI.RockPage.AddOrUpdateCookie( contextCookie );
#endif

            return Ok();
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual bool TryCheckCanEdit( T entity, out IActionResult result )
        {
            if ( entity is ISecured )
            {
                return TryCheckCanEdit( ( ISecured ) entity, out result );
            }

            result = null;

            return true;
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        protected virtual bool TryCheckCanEdit( ISecured securedModel, out IActionResult result )
        {
            return TryCheckCanEdit( securedModel, GetPerson(), out result );
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        /// <param name="person">The person.</param>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        protected virtual bool TryCheckCanEdit( ISecured securedModel, Person person, out IActionResult result )
        {
            if ( securedModel != null )
            {
                if ( IsProxy( securedModel ) )
                {
                    if ( !securedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    {
                        result = Unauthorized();

                        return false;
                    }
                }
                else
                {
                    // Need to reload using service with a proxy enabled so that if model has custom
                    // parent authorities, those properties can be lazy-loaded and checked for authorization
                    SetProxyCreation( true );
                    ISecured reloadedModel = ( ISecured ) Service.Get( securedModel.Id );
                    if ( reloadedModel != null && !reloadedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    {
                        result = Unauthorized();

                        return false;
                    }
                }
            }

            result = null;

            return true;
        }

        /// <summary>
        /// Checks to see if the person is authorized to VIEW
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        /// <param name="person">The person.</param>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        protected virtual bool TryCheckCanView( ISecured securedModel, Person person, out IActionResult result )
        {
            if ( securedModel != null )
            {
                if ( IsProxy( securedModel ) )
                {
                    if ( !securedModel.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        result = Unauthorized();

                        return false;
                    }
                }
                else
                {
                    // Need to reload using service with a proxy enabled so that if model has custom
                    // parent authorities, those properties can be lazy-loaded and checked for authorization
                    SetProxyCreation( true );
                    ISecured reloadedModel = ( ISecured ) Service.Get( securedModel.Id );
                    if ( reloadedModel != null && !reloadedModel.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        result = Unauthorized();

                        return false;
                    }
                }
            }

            result = null;

            return true;
        }
#else
        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual void CheckCanEdit( T entity )
        {
            if ( entity is ISecured )
            {
                CheckCanEdit( ( ISecured ) entity );
            }
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        protected virtual void CheckCanEdit( ISecured securedModel )
        {
            CheckCanEdit( securedModel, GetPerson() );
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        /// <param name="person">The person.</param>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        protected virtual void CheckCanEdit( ISecured securedModel, Person person )
        {
            if ( securedModel != null )
            {
                if ( IsProxy( securedModel ) )
                {
                    if ( !securedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
                else
                {
                    // Need to reload using service with a proxy enabled so that if model has custom
                    // parent authorities, those properties can be lazy-loaded and checked for authorization
                    SetProxyCreation( true );
                    ISecured reloadedModel = ( ISecured ) Service.Get( securedModel.Id );
                    if ( reloadedModel != null && !reloadedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the person is authorized to VIEW
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        /// <param name="person">The person.</param>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        protected virtual void CheckCanView( ISecured securedModel, Person person )
        {
            if ( securedModel != null )
            {
                if ( IsProxy( securedModel ) )
                {
                    if ( !securedModel.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
                else
                {
                    // Need to reload using service with a proxy enabled so that if model has custom
                    // parent authorities, those properties can be lazy-loaded and checked for authorization
                    SetProxyCreation( true );
                    ISecured reloadedModel = ( ISecured ) Service.Get( securedModel.Id );
                    if ( reloadedModel != null && !reloadedModel.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether [enable proxy creation]. This is needed if lazy loading is needed or Editing/Deleting an entity, etc
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable proxy creation]; otherwise, <c>false</c>.
        /// </value>
        protected void SetProxyCreation( bool enabled )
        {
#if NET5_0_OR_GREATER
            Service.Context.ChangeTracker.LazyLoadingEnabled = enabled;
#else
            Service.Context.Configuration.ProxyCreationEnabled = enabled;
#endif
        }

        /// <summary>
        /// Determines whether the specified type is proxy.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected bool IsProxy( object type )
        {
#if NET5_0_OR_GREATER
            return type.GetType().Namespace == "Castle.Proxies";
#else
            return type != null && System.Data.Entity.Core.Objects.ObjectContext.GetObjectType( type.GetType() ) != type.GetType();
#endif
        }
    }
}