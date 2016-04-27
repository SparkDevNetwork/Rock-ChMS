﻿// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

using church.ccv.Steps;
using church.ccv.Steps.Model;
using church.ccv.Datamart;
using System.Drawing;
using System.Data.Entity;
using church.ccv.Datamart.Model;
using Rock.Chart;
using Newtonsoft.Json;

namespace RockWeb.Plugins.church_ccv.Steps
{
    /// <summary>
    /// A block to show a dasboard of measure for the pastor.
    /// </summary>
    [DisplayName( "Steps Taken" )]
    [Category( "CCV > Steps" )]
    [Description( "A block to show the number of steps taken." )]

    public partial class StepsTaken : Rock.Web.UI.RockBlock
    {
        #region Fields

        const string ATTRIBUTE_GLOBAL_TITHE_THRESHOLD = "TitheThreshold";

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the active tab.
        /// </summary>
        /// <value>
        /// The active tab.
        /// </value>
        protected string ActiveTab {
            get
            {
                return ViewState["ActiveTab"].ToString();
            }
            set
            {
                ViewState["ActiveTab"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the date range.
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        protected DateRange DateRange { get; set; }
        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );

            gfStepDetails.ApplyFilterClick += GfStepDetails_ApplyFilterClick;
            gfStepDetails.DisplayFilterValue += GfStepDetails_DisplayFilterValue;
            gStepDetails.GridRebind += gStepDetails_GridRebind;
            gStepDetails.PersonIdField = "PersonId";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            DateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateRange.DelimitedValues );

            if ( !Page.IsPostBack )
            {
                drpDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                drpDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Year;

                DateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateRange.DelimitedValues );

                SetDateLabel();
                LoadCampuses();
                LoadStepTypes();
                LoadPastors();

                ShowCampus();
            }
        }

        

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the Click event of the lbSetDateRange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSetDateRange_Click( object sender, EventArgs e )
        {
            SetDateLabel();
            ShowTab();
        }

        /// <summary>
        /// Handles the Click event of the lbTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTab_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                ActiveTab = lb.ID;
                ShowTab();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpDetailCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpDetailCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowStepDetails();
        }

        /// <summary>
        /// Handles the GridRebind event of the GStepDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void gStepDetails_GridRebind( object sender, GridRebindEventArgs e )
        {
            ShowStepDetails();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the GfStepDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void GfStepDetails_ApplyFilterClick( object sender, EventArgs e )
        {
            gfStepDetails.SaveUserPreference( "Measure Type", ddlMeasureType.SelectedValue != string.Empty ? ddlMeasureType.SelectedValue : string.Empty );

            ShowStepDetails();
        }

        /// <summary>
        /// Gfs the step details_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void GfStepDetails_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Measure Type":
                    int? measureId = e.Value.AsInteger();

                    if ( measureId.HasValue )
                    {
                        e.Value = new StepMeasureService( new RockContext() ).Get( measureId.Value).Title;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpCampusCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampusCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowCampus();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCampusMeasures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCampusMeasures_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = e.Item.DataItem as StepMeasure;

                LineChart lcChart = (LineChart)e.Item.FindControl( "lcMeasure" );
                Literal lChartValue = (Literal)e.Item.FindControl( "lChartValue" );

                if ( !item.IsTbd )
                {
                    var campusId = cpCampusCampus.SelectedValue.AsIntegerOrNull();

                    var chartData = GetChartData( DateRange, measureId: item.Id, campusId: campusId ).ToList();

                    // ensure there is at least one last year date to ensure 2 series
                    if ( !chartData.Any( d => d.SeriesId == "Previous Year" ) && chartData.Count > 0 )
                    {
                        SummaryData blankItem = new SummaryData();
                        blankItem.SeriesId = "Previous Year";
                        blankItem.YValue = 0;
                        blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                        chartData.Add( blankItem );
                    }

                    // sort data
                    chartData = chartData.OrderByDescending( c => c.SeriesId ).ThenBy( c => c.DateTimeStamp ).ToList();

                    if ( chartData.Count() > 0 )
                    {
                        var jsonSetting = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                        };
                        string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                        lcChart.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                        lcChart.Visible = true;
                        lcChart.ChartData = chartDataJson;
                        lcChart.Options.legend.show = false;
                        lcChart.Options.series.lines.lineWidth = 4;
                        lcChart.Options.xaxis.show = false;
                        lcChart.Options.yaxis.show = false;

                        Color measureColor = ColorTranslator.FromHtml( item.Color );
                        Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                        lcChart.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                        lcChart.Options.series.lines.fill = 0;
                        lcChart.Options.grid.show = false;
                        lcChart.Options.series.shadowSize = 0;
                        //lcTest.Options.series.lines.fillColor = "#0f0f0fff";
                    }
                    else
                    {
                        lcChart.Visible = false;
                    }

                    lChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesId == "Current Year" ).Sum( d => d.YValue ));
                } else
                {
                    lcChart.Visible = false;
                    lChartValue.Text = "TBD";
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPastor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPastor_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowPastor( ddlPastor.SelectedValue.AsInteger() );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPastorMeasures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPastorMeasures_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = e.Item.DataItem as StepMeasure;

                LineChart lcChart = (LineChart)e.Item.FindControl( "lcMeasure" );
                Literal lChartValue = (Literal)e.Item.FindControl( "lChartValue" );

                if ( !item.IsTbd )
                {
                    var pastorId = ddlPastor.SelectedValue.AsIntegerOrNull();

                    var chartData = GetChartData( DateRange, measureId: item.Id, pastorId: pastorId ).ToList();

                    // ensure there is at least one last year date to ensure 2 series
                    if ( !chartData.Any( d => d.SeriesId == "Previous Year" ) && chartData.Count > 0 )
                    {
                        SummaryData blankItem = new SummaryData();
                        blankItem.SeriesId = "Previous Year";
                        blankItem.YValue = 0;
                        blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                        chartData.Add( blankItem );
                    }

                    // sort data
                    chartData = chartData.OrderByDescending( c => c.SeriesId ).ThenBy( c => c.DateTimeStamp ).ToList();

                    if ( chartData.Count() > 0 )
                    {
                        var jsonSetting = new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                        };
                        string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                        lcChart.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                        lcChart.Visible = true;
                        lcChart.ChartData = chartDataJson;
                        lcChart.Options.legend.show = false;
                        lcChart.Options.series.lines.lineWidth = 4;
                        lcChart.Options.xaxis.show = false;
                        lcChart.Options.yaxis.show = false;

                        Color measureColor = ColorTranslator.FromHtml( item.Color );
                        Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                        lcChart.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                        lcChart.Options.series.lines.fill = 0;
                        lcChart.Options.grid.show = false;
                        lcChart.Options.series.shadowSize = 0;
                        //lcTest.Options.series.lines.fillColor = "#0f0f0fff";
                    }
                    else
                    {
                        lcChart.Visible = false;
                    }

                    lChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesId == "Current Year" ).Sum( d => d.YValue ) );
                }
                else
                {
                    lcChart.Visible = false;
                    lChartValue.Text = "TBD";
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpAdultsCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpAdultsCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowAdults();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Loads the step types.
        /// </summary>
        private void LoadStepTypes()
        {
            using ( RockContext rockContext = new RockContext() ) {

                var measureTypes = new StepMeasureService( rockContext ).Queryable()
                                    .Where( m => m.IsActive && !m.IsTbd )
                                    .Select( m => new { m.Id, m.Title } )
                                    .OrderBy( m => m.Title);

                ddlMeasureType.DataTextField = "Title";
                ddlMeasureType.DataValueField = "Id";
                ddlMeasureType.DataSource = measureTypes.ToList();
                ddlMeasureType.DataBind();

                ddlMeasureType.Items.Insert( 0, "" );

                ddlMeasureType.SelectedValue = gfStepDetails.GetUserPreference( "Measure Type" );
            }
        }

        /// <summary>
        /// Sets the date label.
        /// </summary>
        private void SetDateLabel()
        {
            if ( DateRange.Start.HasValue && DateRange.End.HasValue )
            {
                hlDate.Text = string.Format( "{0} - {1}", DateRange.Start.Value.ToShortDateString(), DateRange.End.Value.ToShortDateString() );
            }
            else if ( DateRange.Start.HasValue )
            {
                hlDate.Text = string.Format( "Since {0}", DateRange.Start.Value.ToShortDateString() );
            }
            else if ( DateRange.End.HasValue )
            {
                hlDate.Text = string.Format( "Before {0}", DateRange.End.Value.ToShortDateString() );
            }
            else
            {
                hlDate.Text = "No Date Range Provided";
            }
        }

        /// <summary>
        /// Shows the tab.
        /// </summary>
        private void ShowTab()
        {
            liCampus.RemoveCssClass( "active" );
            pnlCampus.Visible = false;

            liPastor.RemoveCssClass( "active" );
            pnlPastor.Visible = false;

            liAdults.RemoveCssClass( "active" );
            pnlAdults.Visible = false;

            liStepDetails.RemoveCssClass( "active" );
            pnlStepDetails.Visible = false;

            switch ( ActiveTab ?? string.Empty )
            {
                case "lbPastor":
                    {
                        liPastor.AddCssClass( "active" );
                        pnlPastor.Visible = true;
                        ShowPastor();
                        break;
                    }

                case "lbAdults":
                    {
                        liAdults.AddCssClass( "active" );
                        pnlAdults.Visible = true;
                        ShowAdults();
                        break;
                    }

                case "lbStepDetails":
                    {
                        liStepDetails.AddCssClass( "active" );
                        pnlStepDetails.Visible = true;
                        ShowStepDetails();
                        break;
                    }

                default:
                    {
                        liCampus.AddCssClass( "active" );
                        pnlCampus.Visible = true;
                        ShowCampus();
                        break;
                    }
            }
        }

        /// <summary>
        /// Loads the campuses.
        /// </summary>
        private void LoadCampuses()
        {
            cpDetailCampus.Campuses = CampusCache.All();
            cpCampusCampus.Campuses = CampusCache.All();
            cpAdultsCampus.Campuses = CampusCache.All();
        }

        /// <summary>
        /// Loads the pastors.
        /// </summary>
        private void LoadPastors()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartNeighborhoodService datamartNeighborhoodService = new DatamartNeighborhoodService( rockContext );

                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var latestMeasureDate = stepMeasureValueService.Queryable().Max( m => m.SundayDate );

                var pastors = datamartNeighborhoodService.Queryable().AsNoTracking()
                                    .Where(n => n.NeighborhoodPastorId != null)
                                    .Select( n => new { n.NeighborhoodPastorName, n.NeighborhoodPastorId } )
                                    .Distinct();                                    

                ddlPastor.DataSource = pastors.OrderBy( p => p.NeighborhoodPastorName ).ToList();
                ddlPastor.DataTextField = "NeighborhoodPastorName";
                ddlPastor.DataValueField = "NeighborhoodPastorId";
                ddlPastor.DataBind();

                if ( ddlPastor.SelectedItem != null )
                {
                    lPastorPastor.Text = ddlPastor.SelectedItem.Text;
                }
            }
        }

        /// <summary>
        /// Shows the adults.
        /// </summary>
        private void ShowAdults(int? campusId = null)
        {
            RockContext rockContext = new RockContext();

            if ( cpAdultsCampus.SelectedCampusId == null )
            {
                lCampusCampus.Text = "All Campuses ";
            }
            else
            {
                campusId = cpAdultsCampus.SelectedItem.Value.AsInteger();
                lAdultsCampus.Text = string.Format( "{0} Campus", cpAdultsCampus.SelectedItem.Text );
            }

            var adultCountResult = GetAdultsCount( DateRange, rockContext, cpAdultsCampus.SelectedValue.AsIntegerOrNull() );

            int uniqueAdults = adultCountResult.UniqueAdults;
            int totalSteps = adultCountResult.TotalSteps;
            double averageSteps = 0;

            if (uniqueAdults != 0 )
            {
                averageSteps = (double)totalSteps / (double)uniqueAdults;
            }

            lAdultUniqueAdults.Text = uniqueAdults.ToString();
            lAdultsTotalSteps.Text = totalSteps.ToString();
            lAdultsAvergeSteps.Text = averageSteps != 0 ? Math.Round( averageSteps, 1).ToString() : "NA";
        }

        /// <summary>
        /// Shows the pastor.
        /// </summary>
        /// <param name="pastorId">The pastor identifier.</param>
        private void ShowPastor(int? pastorId = null )
        {
            RockContext rockContext = new RockContext();

            pastorId = ddlPastor.SelectedItem.Value.AsInteger();
            lPastorPastor.Text = ddlPastor.SelectedItem.Text;
            
            var measures = new StepMeasureService( rockContext ).Queryable().AsNoTracking().Where( m => m.IsActive ).ToList();
            rptPastorMeasures.DataSource = measures;
            rptPastorMeasures.DataBind();
        }

        /// <summary>
        /// Shows the campus.
        /// </summary>
        private void ShowCampus(int? campusId = null)
        {
            RockContext rockContext = new RockContext();

            if ( cpDetailCampus.SelectedCampusId == null )
            {
                lCampusCampus.Text = "All Campuses ";
            }
            else
            {
                campusId = cpCampusCampus.SelectedItem.Value.AsInteger();
                lCampusCampus.Text = string.Format( "{0} Campus", cpCampusCampus.SelectedItem.Text );
            }

            var measures = new StepMeasureService( rockContext ).Queryable().AsNoTracking().Where( m => m.IsActive ).ToList();
            rptCampusMeasures.DataSource = measures;
            rptCampusMeasures.DataBind();
        }

        /// <summary>
        /// Shows the step details.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        private void ShowStepDetails(int? campusId = null )
        {
            if ( cpDetailCampus.SelectedCampusId == null )
            {
                lDetailCampus.Text = "All Campuses ";
            }
            else
            {
                campusId = cpDetailCampus.SelectedItem.Value.AsInteger();
                lDetailCampus.Text = string.Format( "{0} Campus", cpDetailCampus.SelectedItem.Text );
            }

            using (RockContext rockContext = new RockContext() )
            {
                StepTakenService stepTakenService = new StepTakenService( rockContext );

                var query = stepTakenService.Queryable("StepMeasure").AsNoTracking();
                
                if ( campusId.HasValue )
                {
                    query = query.Where( s => s.CampusId == campusId.Value );
                }
                
                if ( DateRange != null && DateRange.Start.HasValue )
                {
                    query = query.Where( s => s.DateTaken >= DateRange.Start.Value );
                }
                
                if ( DateRange != null && DateRange.End.HasValue )
                {
                    query = query.Where( s => s.DateTaken <= DateRange.End.Value );
                }

                if ( !string.IsNullOrWhiteSpace(ddlMeasureType.SelectedValue ))
                {
                    int? measureId = ddlMeasureType.SelectedValue.AsInteger();
                    query = query.Where( s => s.StepMeasureId == measureId );
                }

                // join to datamart
                decimal titheThreshold = GlobalAttributesCache.Read().GetValue( ATTRIBUTE_GLOBAL_TITHE_THRESHOLD ).AsDecimal();

                var datamartQry = new DatamartPersonService( rockContext ).Queryable().AsNoTracking();

                var joinedQuery = query.GroupJoin(
                        datamartQry,
                        s => s.PersonAlias.PersonId,
                        d => d.PersonId,
                        ( s, d ) => new { s, d }
                    )
                    .SelectMany( x => x.d.DefaultIfEmpty(), ( g, u ) => new { Step = g.s, Datamart = u } );

                var results = joinedQuery.Select( s =>
                                new {
                                    Id = s.Step.Id,
                                    DateTaken = s.Step.DateTaken,
                                    StepMeasureTitle = s.Step.StepMeasure.Title,
                                    StepMeasureId = s.Step.StepMeasureId,
                                    SundayDate = s.Step.SundayDate,
                                    PersonId = s.Step.PersonAlias.PersonId,
                                    FirstName = s.Step.PersonAlias.Person.FirstName,
                                    NickName = s.Step.PersonAlias.Person.NickName,
                                    LastName = s.Step.PersonAlias.Person.LastName,
                                    FullName = s.Step.PersonAlias.Person.LastName + ", " + s.Step.PersonAlias.Person.NickName,
                                    Campus = s.Step.Campus.Name,
                                    Address = s.Datamart.Address,
                                    State = s.Datamart.State, 
                                    City = s.Datamart.City,
                                    PostalCode = s.Datamart.PostalCode,
                                    Email = s.Datamart.Email,
                                    ConnectionStatus = s.Datamart.ConnectionStatusName,
                                    FamilyRole = s.Datamart.FamilyRole,
                                    FirstVisitDate = s.Datamart.FirstVisitDate, 
                                    BaptismDate = s.Datamart.BaptismDate, 
                                    StartingPointDate = s.Datamart.StartingPointDate, 
                                    InNeighborhoodGroup = s.Datamart.InNeighborhoodGroup, 
                                    IsServing = s.Datamart.IsServing,
                                    IsEra = s.Datamart.IsEra,
                                    IsCoaching = s.Datamart.IsCoaching,
                                    IsGiving = (s.Datamart.GivingLast12Months != null) ? (s.Datamart.GivingLast12Months.Value > titheThreshold) : false
                                }
                );

                gStepDetails.SetLinqDataSource( results.OrderBy(s => s.DateTaken) );
                gStepDetails.DataBind();
            }
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="measureId">The measure identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="pastorId">The pastor identifier.</param>
        /// <returns></returns>
        private IEnumerable<IChartData> GetChartData(DateRange dateRange, RockContext rockContext = null, int? measureId = null, int? campusId = null, int? pastorId = null )
        {
            if (rockContext == null )
            {
                rockContext = new RockContext();
            }

            bool showPreviousYear = false;
            
            if (dateRange.Start.HasValue && dateRange.End.HasValue )
            {
                showPreviousYear = true;
            }

            var currentData = new StepTakenService( rockContext ).Queryable();
            var previousYearData = new StepTakenService( rockContext ).Queryable();

            if ( dateRange.Start.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken >= dateRange.Start );
                var previousYearDate = dateRange.Start.Value.AddYears( -1 );
                previousYearData = previousYearData.Where( s => s.DateTaken >= previousYearDate );
            }

            if ( dateRange.End.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken <= dateRange.End );
                var previousYearDate = dateRange.End.Value.AddYears( -1 );
                previousYearData = previousYearData.Where( s => s.DateTaken <= previousYearDate );
            }

            if ( measureId.HasValue )
            {
                currentData = currentData.Where( s => s.StepMeasureId == measureId.Value );
                previousYearData = previousYearData.Where( s => s.StepMeasureId == measureId.Value );
            }

            if ( campusId.HasValue )
            {
                currentData = currentData.Where( s => s.CampusId == campusId );
                previousYearData = previousYearData.Where( s => s.CampusId == campusId );
            }

            // filter by pastor
            if ( pastorId.HasValue )
            {
                // let the unholiness begin as we join to the datamart person and datamart neighborhood tables to find the pastor
                var datamartNeighborhoodQry = new DatamartNeighborhoodService( rockContext ).Queryable();
                var datamartPersonQry = new DatamartPersonService( rockContext ).Queryable().AsNoTracking()
                                            .Join( datamartNeighborhoodQry,
                                                    x => x.NeighborhoodId,
                                                    y => y.NeighborhoodId,
                                                    ( x, y ) => new { Person = x, Neighborhood = y } );

                currentData = currentData.Join(
                        datamartPersonQry,
                        s => s.PersonAlias.PersonId,
                        d => d.Person.PersonId,
                        ( s, d ) => new { Step = s, Datamart = d }
                    )
                    .Where(g => g.Datamart.Neighborhood.NeighborhoodPastorId == pastorId)
                    .Select(g => g.Step);

                previousYearData = previousYearData.Join(
                        datamartPersonQry,
                        s => s.PersonAlias.PersonId,
                        d => d.Person.PersonId,
                        ( s, d ) => new { Step = s, Datamart = d }
                    )
                    .Where( g => g.Datamart.Neighborhood.NeighborhoodPastorId == pastorId )
                    .Select( g => g.Step );
            }

            var returnData = currentData.GroupBy( s => s.SundayDate )
                            .Select( g => new
                            {
                                DateKey = g.Key.Value,
                                Count = g.Count()
                            } )
                            .ToList()
                            .Select( c => new SummaryData
                            {
                                DateTimeStamp = c.DateKey.ToJavascriptMilliseconds(),
                                DateTime = c.DateKey,
                                SeriesId = "Current Year",
                                YValue = c.Count
                            } ).ToList();

            if ( showPreviousYear )
            {
                var previousYearReturnData = previousYearData.GroupBy( s => s.SundayDate )
                            .Select( g => new
                            {
                                DateKey = g.Key.Value,
                                Count = g.Count()
                            } )
                            .ToList()
                            .Select( c => new SummaryData
                            {
                                DateTimeStamp = c.DateKey.AddYears(1).ToJavascriptMilliseconds(),
                                DateTime = c.DateKey.AddYears(1),
                                SeriesId = "Previous Year",
                                YValue = c.Count
                            } ).ToList();

                returnData = returnData.Union( previousYearReturnData ).ToList();
            }

            return returnData;
        }

        /// <summary>
        /// Gets the adults count.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        private AdultCountResult GetAdultsCount( DateRange dateRange, RockContext rockContext = null, int? campusId = null )
        {
            AdultCountResult result = new AdultCountResult();

            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var currentData = new StepTakenService( rockContext ).Queryable();

            if ( dateRange.Start.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken >= dateRange.Start );
            }

            if ( dateRange.End.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken <= dateRange.End );
            }

            if ( campusId.HasValue )
            {
                currentData = currentData.Where( s => s.CampusId == campusId );
            }

            // join to datamart to get family status 
            var datamartQry = new DatamartPersonService( rockContext ).Queryable().AsNoTracking();

            var joinedQuery = currentData.GroupJoin(
                    datamartQry,
                    s => s.PersonAlias.PersonId,
                    d => d.PersonId,
                    ( s, d ) => new { s, d }
                )
                .SelectMany( x => x.d.DefaultIfEmpty(), ( g, u ) => new { Step = g.s, Datamart = u } );

            result.UniqueAdults = joinedQuery.Where( j => j.Datamart.FamilyRole == "Adult" ).Select( j => j.Datamart.PersonId ).Distinct().Count();
            result.TotalSteps = joinedQuery.Where(j => j.Datamart.FamilyRole == "Adult").Count();

            return result;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class AdultCountResult
    {
        /// <summary>
        /// Gets or sets the unique adults.
        /// </summary>
        /// <value>
        /// The unique adults.
        /// </value>
        public int UniqueAdults { get; set; }
        /// <summary>
        /// Gets or sets the total steps.
        /// </summary>
        /// <value>
        /// The total steps.
        /// </value>
        public int TotalSteps { get; set; }
    }
}