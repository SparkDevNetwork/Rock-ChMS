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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [DisplayName( "Attributes" )]
    [Category( "Core" )]
    [Description( "Allows for the managing of attribues." )]

    [EntityTypeField( "Entity", "Entity Name", false, "Applies To", 0 )]
    [TextField( "Entity Qualifier Column", "The entity column to evaluate when determining if this attribute applies to the entity", false, "", "Applies To", 1 )]
    [TextField( "Entity Qualifier Value", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "", "Applies To", 2 )]
    [BooleanField( "Allow Setting of Values", "Should UI be available for setting values of the specified Entity ID?", false, order: 3 )]
    [IntegerField( "Entity Id", "The entity id that values apply to", false, 0, order: 4 )]
    [BooleanField( "Enable Show In Grid", "Should the 'Show In Grid' option be displayed when editing attributes?", false, order: 5 )]
    [TextField( "Category Filter", "A comma separated list of category guids to limit the display of attributes to.", false, "", order: 6 )]

    public partial class Attributes : RockBlock, ICustomGridColumns
    {
        #region Fields

        private int? _entityTypeId = null;
        private string _entityQualifierColumn = string.Empty;
        private string _entityQualifierValue = string.Empty;
        private bool _displayValueEdit = false;
        private int? _entityId = null;
        private bool _canConfigure = false;
        private bool _isEntityTypeConfigured = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // if limiting by category list hide the filters
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "CategoryFilter" ) ) )
            {
                rFilter.Visible = false;
            }

            edtAttribute.IsShowInGridVisible = GetAttributeValue( "EnableShowInGrid" ).AsBooleanOrNull() ?? false;

            Guid? entityTypeGuid = GetAttributeValue( "Entity" ).AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                _isEntityTypeConfigured = true;
                if ( default( Guid ) == entityTypeGuid )
                {
                    _entityTypeId = default( int );
                }
                else
                {
                    _entityTypeId = CacheEntityType.Get( entityTypeGuid.Value ).Id;
                }
            }
            else
            {
                _entityTypeId = rFilter.GetUserPreference( "Entity Type" ).AsIntegerOrNull();
                var entityTypeList = new EntityTypeService( new RockContext() ).GetEntities().ToList();
                ddlEntityType.EntityTypes = entityTypeList;
                ddlAttrEntityType.EntityTypes = entityTypeList;
                ddlEntityType.SetValue( _entityTypeId );

            }

            _entityQualifierColumn = GetAttributeValue( "EntityQualifierColumn" );
            _entityQualifierValue = GetAttributeValue( "EntityQualifierValue" );
            _displayValueEdit = GetAttributeValue( "AllowSettingofValues" ).AsBooleanOrNull() ?? false;

            _entityId = GetAttributeValue( "EntityId" ).AsIntegerOrNull();
            if ( _entityId == 0 )
            {
                _entityId = null;
            }

            _canConfigure = IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE );


            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            if ( _canConfigure )
            {
                rGrid.DataKeyNames = new string[] { "Id" };
                rGrid.Actions.ShowAdd = true;
                rGrid.GridReorder += RGrid_GridReorder;
                rGrid.Actions.AddClick += rGrid_Add;
                rGrid.GridRebind += rGrid_GridRebind;
                rGrid.RowDataBound += rGrid_RowDataBound;

                var lEntityQualifierField = rGrid.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lEntityQualifier" );
                if ( lEntityQualifierField != null )
                {
                    lEntityQualifierField.Visible = !_entityId.HasValue;   // qualifier
                }

                var rtDefaultValueField = rGrid.ColumnsOfType<RockTemplateField>().FirstOrDefault( a => a.ID == "rtDefaultValue" );
                if ( rtDefaultValueField != null )
                {
                    rtDefaultValueField.Visible = !_displayValueEdit; // default value / value
                }

                var rtValueField = rGrid.ColumnsOfType<RockTemplateField>().FirstOrDefault( a => a.ID == "rtValue" );
                if ( rtValueField != null )
                {
                    rtValueField.Visible = _displayValueEdit; // default value / value
                }

                var editField = rGrid.ColumnsOfType<EditField>().FirstOrDefault();
                if ( editField != null )
                {
                    editField.Visible = _displayValueEdit; // edit
                }

                var securityField = rGrid.ColumnsOfType<SecurityField>().FirstOrDefault();
                if ( securityField != null )
                {
                    securityField.EntityTypeId = CacheEntityType.Get( typeof( Rock.Model.Attribute ) ).Id;
                }

                mdAttribute.SaveClick += mdAttribute_SaveClick;
                mdAttributeValue.SaveClick += mdAttributeValue_SaveClick;



                BindFilter();
            }
            else
            {
                nbMessage.Text = "You are not authorized to configure this page";
                nbMessage.Visible = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _canConfigure && IsEntityTypeValid() )
                {
                    BindGrid();
                }
            }
            else
            {
                int? attributeId = hfIdValues.Value.AsIntegerOrNull();
                if ( attributeId.HasValue )
                {
                    ShowEditValue( attributeId.Value, false );
                }

                if ( hfActiveDialog.Value.ToUpper() == "ATTRIBUTEVALUE" )
                {
                    //
                }

                ShowDialog();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Entity Type", ddlEntityType.SelectedValue );
            _entityTypeId = ddlEntityType.SelectedValue.AsIntegerOrNull();
            if ( IsEntityTypeValid() )
            {
                BindFilterForSelectedEntityType();
                BindGrid();
            }

        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            string categoryFilterValue = cpCategoriesFilter.SelectedValuesAsInt()
                .Where( v => v != 0 )
                .Select( c => c.ToString() )
                .ToList()
                .AsDelimited( "," );

            rFilter.SaveUserPreference( "Categories", categoryFilterValue );
            rFilter.SaveUserPreference( "Analytics Enabled", ddlAnalyticsEnabled.SelectedValue );
            rFilter.SaveUserPreference( "Active", ddlActiveFilter.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Categories":

                    var categories = new List<string>();

                    foreach ( var idVal in e.Value.SplitDelimitedValues() )
                    {
                        int id = int.MinValue;
                        if ( int.TryParse( idVal, out id ) )
                        {
                            if ( id != 0 )
                            {
                                var category = CacheCategory.Get( id );
                                if ( category != null )
                                {
                                    categories.Add( CacheCategory.Get( id ).Name );
                                }
                            }
                        }
                    }

                    e.Value = categories.AsDelimited( ", " );

                    break;

                case "Analytics Enabled":

                    if ( !ddlAnalyticsEnabled.Visible )
                    {
                        e.Value = string.Empty;
                    }
                    else
                    {
                        var filterValue = e.Value.AsBooleanOrNull();
                        e.Value = filterValue.HasValue ? filterValue.Value.ToYesNo() : null;
                    }

                    break;

                case "Active":

                    if ( !ddlActiveFilter.Visible )
                    {
                        e.Value = string.Empty;
                    }
                    else
                    {
                        var filterValue = e.Value.AsBooleanOrNull();
                        e.Value = filterValue.HasValue ? filterValue.Value.ToYesNo() : null;
                    }

                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the EditValue event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowSelected( object sender, RowEventArgs e )
        {
            if ( _displayValueEdit )
            {
                ShowEditValue( e.RowKeyId, true );
            }
            else
            {
                ShowEdit( e.RowKeyId );
            }
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            var attribute = attributeService.Get( e.RowKeyId );
            if ( attribute != null )
            {
                CacheAttribute.Remove( attribute.Id );

                if ( ( !_entityTypeId.HasValue || _entityTypeId.Value == 0 ) &&
                     _entityQualifierColumn == string.Empty &&
                     _entityQualifierValue == string.Empty &&
                     ( !_entityId.HasValue || _entityId.Value == 0 )
                )
                {
                    CacheGlobalAttributes.Remove();
                }

                attributeService.Delete( attribute );

                rockContext.SaveChanges();
            }

            CacheAttribute.RemoveEntityAttributes();

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void rGrid_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int attributeId = ( int ) rGrid.DataKeys[e.Row.RowIndex].Value;

                var attribute = Rock.Cache.CacheAttribute.Get( attributeId );
                var fieldType = Rock.Cache.CacheFieldType.Get( attribute.FieldTypeId );

                Literal lCategories = e.Row.FindControl( "lCategories" ) as Literal;
                if ( lCategories != null )
                {
                    lCategories.Text = attribute.Categories.Select( c => c.Name ).ToList().AsDelimited( ", " );
                }

                Literal lEntityQualifier = e.Row.FindControl( "lEntityQualifier" ) as Literal;
                if ( lEntityQualifier != null )
                {
                    if ( attribute.EntityTypeId.HasValue )
                    {
                        string entityTypeName = CacheEntityType.Get( attribute.EntityTypeId.Value ).FriendlyName;
                        if ( !string.IsNullOrWhiteSpace( attribute.EntityTypeQualifierColumn ) )
                        {
                            lEntityQualifier.Text = string.Format( "{0} where [{1}] = '{2}'", entityTypeName, attribute.EntityTypeQualifierColumn, attribute.EntityTypeQualifierValue );
                        }
                        else
                        {
                            lEntityQualifier.Text = entityTypeName;
                        }
                    }
                    else
                    {
                        lEntityQualifier.Text = "Global Attribute";
                    }
                }

                if ( _displayValueEdit )
                {
                    Literal lValue = e.Row.FindControl( "lValue" ) as Literal;
                    if ( lValue != null )
                    {
                        AttributeValueService attributeValueService = new AttributeValueService( new RockContext() );
                        var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId );
                        if ( attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) )
                        {
                            lValue.Text = fieldType.Field.FormatValue( lValue, attribute.EntityTypeId, _entityId, attributeValue.Value, attribute.QualifierValues, true ).EncodeHtml();
                        }
                        else
                        {
                            lValue.Text = string.Format( "<span class='text-muted'>{0}</span>", fieldType.Field.FormatValue( lValue, attribute.EntityTypeId, _entityId, attribute.DefaultValue, attribute.QualifierValues, true ).EncodeHtml() );
                        }
                    }
                }
                else
                {
                    Literal lDefaultValue = e.Row.FindControl( "lDefaultValue" ) as Literal;
                    if ( lDefaultValue != null )
                    {
                        lDefaultValue.Text = fieldType.Field.FormatValueAsHtml( lDefaultValue, attribute.EntityTypeId, _entityId, attribute.DefaultValue, attribute.QualifierValues, true );
                    }
                }

                if ( attribute.IsActive == false )
                {
                    e.Row.AddCssClass( "is-inactive" );
                }
            }
        }


        private void RGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var qry = GetData( rockContext );
            var updatedAttributeIds = attributeService.Reorder( qry.ToList(), e.OldIndex, e.NewIndex );

            rockContext.SaveChanges();

            foreach ( int id in updatedAttributeIds )
            {
                CacheAttribute.Remove( id );
            }

            CacheGlobalAttributes.Remove();
            CacheAttribute.RemoveEntityAttributes();


            BindGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlAttrEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlAttrEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            edtAttribute.AttributeEntityTypeId = ddlAttrEntityType.SelectedValueAsInt( false );
            edtAttribute.CategoryIds = new List<int>();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = null;

            if ( _isEntityTypeConfigured )
            {
                var entityTypeId = _entityTypeId.HasValue && _entityTypeId > 0 ? _entityTypeId : null;
                attribute = Helper.SaveAttributeEdits( edtAttribute, entityTypeId, _entityQualifierColumn, _entityQualifierValue );
            }
            else
            {
                attribute = Helper.SaveAttributeEdits( edtAttribute, ddlAttrEntityType.SelectedValueAsInt(), tbAttrQualifierField.Text, tbAttrQualifierValue.Text );
            }

            // Attribute will be null if it was not valid
            if ( attribute == null )
            {
                return;
            }

            CacheAttribute.RemoveEntityAttributes();

            HideDialog();

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAttributeValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdAttributeValue_SaveClick( object sender, EventArgs e )
        {
            if ( _displayValueEdit )
            {
                int attributeId = 0;
                if ( hfIdValues.Value != string.Empty && !int.TryParse( hfIdValues.Value, out attributeId ) )
                {
                    attributeId = 0;
                }

                if ( attributeId != 0 && phEditControls.Controls.Count > 0 )
                {
                    var attribute = Rock.Cache.CacheAttribute.Get( attributeId );

                    var rockContext = new RockContext();
                    var attributeValueService = new AttributeValueService( rockContext );
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attributeId, _entityId );
                    if ( attributeValue == null )
                    {
                        attributeValue = new AttributeValue
                        {
                            AttributeId = attributeId,
                            EntityId = _entityId
                        };
                        attributeValueService.Add( attributeValue );
                    }

                    var fieldType = CacheFieldType.Get( attribute.FieldType.Id );
                    attributeValue.Value = fieldType.Field.GetEditValue( attribute.GetControl( phEditControls.Controls[0] ), attribute.QualifierValues );

                    rockContext.SaveChanges();

                    CacheAttribute.Remove( attributeId );

                    if ( ( !_entityTypeId.HasValue || _entityTypeId.Value == 0 ) &&
                         _entityQualifierColumn == string.Empty && 
                         _entityQualifierValue == string.Empty && 
                         ( !_entityId.HasValue || _entityId.Value == 0 ) 
                    )
                    {
                        CacheGlobalAttributes.Remove();
                    }

                    CacheAttribute.RemoveEntityAttributes();
                }

                hfIdValues.Value = string.Empty;

                HideDialog();
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlEntityType.Visible = !_isEntityTypeConfigured;
            ddlEntityType.SetValue( rFilter.GetUserPreference( "Entity Type" ) );
            BindFilterForSelectedEntityType();
        }

        /// <summary>
        /// Binds the type of the filter for selected entity.
        /// </summary>
        private void BindFilterForSelectedEntityType()
        {
            var entityTypeCache = _isEntityTypeConfigured ? CacheEntityType.Get( _entityTypeId.Value ) : null;
            ddlAnalyticsEnabled.Visible = entityTypeCache != null && entityTypeCache.IsAnalyticsSupported( null, null );
            ddlAnalyticsEnabled.SetValue( rFilter.GetUserPreference( "Analytics Enabled" ) );

            ddlActiveFilter.SetValue( rFilter.GetUserPreference( "Active" ) );

            BindCategoryFilter();
        }

        /// <summary>
        /// Binds the category filter.
        /// </summary>
        private void BindCategoryFilter()
        {
            cpCategoriesFilter.EntityTypeId = CacheEntityType.Get( typeof( Rock.Model.Attribute ) ).Id;
            cpCategoriesFilter.EntityTypeQualifierColumn = "EntityTypeId";
            cpCategoriesFilter.EntityTypeQualifierValue = _entityTypeId.ToString();

            var selectedIDs = new List<int>();

            if ( ( _entityTypeId ?? 0 ).ToString() == rFilter.GetUserPreference( "Entity Type" ) )
            {
                foreach ( var idVal in rFilter.GetUserPreference( "Categories" ).SplitDelimitedValues() )
                {
                    int id = int.MinValue;
                    if ( int.TryParse( idVal, out id ) )
                    {
                        selectedIDs.Add( id );
                    }
                }
            }

            cpCategoriesFilter.SetValues( selectedIDs );
        }

        private IQueryable<Rock.Model.Attribute> GetData( RockContext rockContext )
        {
            IQueryable<Rock.Model.Attribute> query = null;

            AttributeService attributeService = new AttributeService( rockContext );
            if ( _entityTypeId.HasValue )
            {
                if ( _entityTypeId == default( int ) )
                {
                    query = attributeService.GetByEntityTypeId( null, true );
                }
                else
                {
                    query = attributeService.GetByEntityTypeQualifier( _entityTypeId, _entityQualifierColumn, _entityQualifierValue, true );
                }
            }

            // if filtering by block setting of categories
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "CategoryFilter" ) ) )
            {
                try
                {
                    var categoryGuids = GetAttributeValue( "CategoryFilter" ).Split( ',' ).Select( Guid.Parse ).ToList();

                    query = query.Where( a => a.Categories.Any( c => categoryGuids.Contains( c.Guid ) ) );
                }
                catch { }
            }

            if ( ddlAnalyticsEnabled.Visible )
            {
                var filterValue = ddlAnalyticsEnabled.SelectedValue.AsBooleanOrNull();
                if ( filterValue.HasValue )
                {
                    query = query.Where( a => ( a.IsAnalytic || a.IsAnalyticHistory ) == filterValue );
                }
            }

            if ( ddlActiveFilter.Visible )
            {
                var filterValue = ddlActiveFilter.SelectedValue.AsBooleanOrNull();
                if ( filterValue.HasValue )
                {
                    query = query.Where( a => a.IsActive == filterValue );
                }
            }

            var selectedCategoryIds = new List<int>();
            rFilter.GetUserPreference( "Categories" ).SplitDelimitedValues().ToList().ForEach( s => selectedCategoryIds.Add( int.Parse( s ) ) );
            if ( selectedCategoryIds.Any() )
            {
                query = query.Where( a => a.Categories.Any( c => selectedCategoryIds.Contains( c.Id ) ) );
            }

            query = query.OrderBy( a => a.Order );

            return query;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var query = GetData( new RockContext() );
            rGrid.DataSource = query.ToList();
            rGrid.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int attributeId )
        {
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var attributeModel = attributeService.Get( attributeId );

            if ( attributeModel == null )
            {
                mdAttribute.Title = "Add Attribute".FormatAsHtmlTitle();

                attributeModel = new Rock.Model.Attribute();
                attributeModel.FieldTypeId = CacheFieldType.Get( Rock.SystemGuid.FieldType.TEXT ).Id;

                if ( !_isEntityTypeConfigured )
                {
                    int entityTypeId = int.MinValue;
                    if ( int.TryParse( rFilter.GetUserPreference( "Entity Type" ), out entityTypeId ) && entityTypeId > 0 )
                    {
                        attributeModel.EntityTypeId = entityTypeId;
                    }
                }
                else
                {
                    attributeModel.EntityTypeId = _entityTypeId;
                    attributeModel.EntityTypeQualifierColumn = _entityQualifierColumn;
                    attributeModel.EntityTypeQualifierValue = _entityQualifierValue;
                }

                List<int> selectedCategoryIds = cpCategoriesFilter.SelectedValuesAsInt().ToList();
                new CategoryService( rockContext ).Queryable().Where( c => selectedCategoryIds.Contains( c.Id ) ).ToList().ForEach( c =>
                    attributeModel.Categories.Add( c ) );
                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Add( Rock.Model.Attribute.FriendlyTypeName );
            }
            else
            {
                if ( attributeModel.EntityType != null && attributeModel.EntityType.IsIndexingSupported == true && attributeModel.EntityType.IsIndexingEnabled )
                {
                    edtAttribute.IsIndexingEnabledVisible = true;
                }

                edtAttribute.ActionTitle = Rock.Constants.ActionTitle.Edit( Rock.Model.Attribute.FriendlyTypeName );
                mdAttribute.Title = ( "Edit " + attributeModel.Name ).FormatAsHtmlTitle();

                edtAttribute.IsIndexingEnabled = attributeModel.IsIndexEnabled;
            }

            Type type = null;
            if ( attributeModel.EntityTypeId.HasValue && attributeModel.EntityTypeId > 0 )
            {
                type = CacheEntityType.Get( attributeModel.EntityTypeId.Value ).GetEntityType();
            }
            edtAttribute.ReservedKeyNames = attributeService.GetByEntityTypeQualifier( attributeModel.EntityTypeId, attributeModel.EntityTypeQualifierColumn, attributeModel.EntityTypeQualifierValue, true )
                 .Where( a => a.Id != attributeId )
                 .Select( a => a.Key )
                 .Distinct()
                 .ToList();
            edtAttribute.SetAttributeProperties( attributeModel, type );
            edtAttribute.AttributeEntityTypeId = attributeModel.EntityTypeId;

            if ( _isEntityTypeConfigured )
            {
                pnlEntityTypeQualifier.Visible = false;
            }
            else
            {
                pnlEntityTypeQualifier.Visible = true;

                ddlAttrEntityType.SetValue( attributeModel.EntityTypeId.HasValue ? attributeModel.EntityTypeId.Value.ToString() : "0" );
                tbAttrQualifierField.Text = attributeModel.EntityTypeQualifierColumn;
                tbAttrQualifierValue.Text = attributeModel.EntityTypeQualifierValue;
            }

            ShowDialog( "Attribute", true );
        }

        /// <summary>
        /// Shows the edit value.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void ShowEditValue( int attributeId, bool setValues )
        {
            if ( _displayValueEdit )
            {
                phEditControls.Controls.Clear();

                var attribute = Rock.Cache.CacheAttribute.Get( attributeId );
                if ( attribute != null )
                {
                    mdAttributeValue.Title = attribute.Name + " Value";

                    var attributeValue = new AttributeValueService( new RockContext() ).GetByAttributeIdAndEntityId( attributeId, _entityId );
                    string value = attributeValue != null && !string.IsNullOrWhiteSpace( attributeValue.Value ) ? attributeValue.Value : attribute.DefaultValue;
                    attribute.AddControl( phEditControls.Controls, value, string.Empty, setValues, true );

                    SetValidationGroup( phEditControls.Controls, mdAttributeValue.ValidationGroup );

                    if ( setValues )
                    {
                        hfIdValues.Value = attribute.Id.ToString();
                        ShowDialog( "AttributeValue", true );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTE":
                    mdAttribute.Show();
                    break;
                case "ATTRIBUTEVALUE":
                    mdAttributeValue.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTE":
                    mdAttribute.Hide();
                    break;
                case "ATTRIBUTEVALUE":
                    mdAttributeValue.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Check if Entity Type Is Valid
        /// </summary>
        private bool IsEntityTypeValid()
        {
            if ( _entityTypeId.HasValue )
            {
                pnlGrid.Visible = true;
                nbMessage.Visible = false;
                return true;
            }
            else
            {
                pnlGrid.Visible = false;
                nbMessage.Text = "Please select an entity to display attributes for.";
                nbMessage.Visible = true;
                return false;
            }
        }

        #endregion
    }
}