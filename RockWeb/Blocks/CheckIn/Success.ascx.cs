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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Achievement;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Success" )]
    [Category( "Check-in" )]
    [Description( "Displays the details of a successful checkin." )]

    #region Block Attributes

    [LinkedPage( "Person Select Page",
        Key = AttributeKey.PersonSelectPage,
        IsRequired = false,
        Order = 5 )]

    [TextField( "Title",
        Key = AttributeKey.Title,
        IsRequired = false,
        DefaultValue = "Checked-in",
        Category = "Text",
        Order = 6 )]

    [TextField( "Detail Message",
        Key = AttributeKey.DetailMessage,
        Description = "The message to display indicating person has been checked in. Use {0} for person, {1} for group, {2} for schedule, and {3} for the security code",
        IsRequired = false,
        DefaultValue = "{0} was checked into {1} in {2} at {3}",
        Category = "Text",
        Order = 7 )]

    #endregion Block Attributes

    public partial class Success : CheckInBlock
    {
        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlock also has inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string PersonSelectPage = "PersonSelectPage";
            public const string Title = "Title";
            public const string DetailMessage = "DetailMessage";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-success-bg" );
            }
        }

        /// <summary>
        /// CheckinResult for rendering the Success Lava Template
        /// </summary>
        /// <seealso cref="RockDynamic" />
        public class CheckinResult : RockDynamic
        {
            /// <summary>
            /// Gets the person.
            /// </summary>
            /// <value>
            /// The person.
            /// </value>
            public CheckInPerson Person { get; internal set; }

            /// <summary>
            /// Gets the group.
            /// </summary>
            /// <value>
            /// The group.
            /// </value>
            public CheckInGroup Group { get; internal set; }

            /// <summary>
            /// Gets the location.
            /// </summary>
            /// <value>
            /// The location.
            /// </value>
            public Location Location { get; internal set; }

            /// <summary>
            /// Gets the schedule.
            /// </summary>
            /// <value>
            /// The schedule.
            /// </value>
            public CheckInSchedule Schedule { get; internal set; }

            /// <summary>
            /// Gets the detail message.
            /// </summary>
            /// <value>
            /// The detail message.
            /// </value>
            public string DetailMessage { get; internal set; }

            /// <summary>
            /// Gets the in progress achievement attempts.
            /// </summary>
            /// <value>
            /// The in progress achievement attempts.
            /// </value>
            public AchievementAttempt[] InProgressAchievementAttempts { get; internal set; }

            /// <summary>
            /// Gets the just completed achievement attempts.
            /// </summary>
            /// <value>
            /// The just completed achievement attempts.
            /// </value>
            public AchievementAttempt[] JustCompletedAchievementAttempts { get; internal set; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    try
                    {
                        ShowDetails();
                    }
                    catch ( Exception ex )
                    {
                        LogException( ex );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            lTitle.Text = GetAttributeValue( AttributeKey.Title );
            string detailMsg = GetAttributeValue( AttributeKey.DetailMessage );

            var printFromClient = new List<CheckInLabel>();
            var printFromServer = new List<CheckInLabel>();

            List<CheckinResult> checkinResultList = new List<CheckinResult>();

            var inProgressAchievementAttemptsByPersonId = CurrentCheckInState.CheckIn.InProgressAchievementAttemptsByPersonId;
            var justCompletedAchievementAttemptsByPersonId = CurrentCheckInState.CheckIn.JustCompletedAchievementAttemptsByPersonId;

            // Populate Checkin Results and label data
            foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
            {
                lbAnother.Visible =
                    CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Individual &&
                    family.People.Count > 1;

                foreach ( var person in family.GetPeople( true ) )
                {
                    foreach ( var groupType in person.GetGroupTypes( true ) )
                    {
                        foreach ( var group in groupType.GetGroups( true ) )
                        {
                            foreach ( var location in group.GetLocations( true ) )
                            {
                                foreach ( var schedule in location.GetSchedules( true ) )
                                {
                                    string detailMessage = string.Format( detailMsg, person.ToString(), group.ToString(), location.Location.Name, schedule.ToString(), person.SecurityCode );
                                    CheckinResult checkinResult = new CheckinResult();
                                    checkinResult.Person = person;
                                    checkinResult.Group = group;
                                    checkinResult.Location = location.Location;
                                    checkinResult.Schedule = schedule;
                                    checkinResult.DetailMessage = detailMessage;
                                    checkinResult.InProgressAchievementAttempts = inProgressAchievementAttemptsByPersonId?.GetValueOrNull( person.Person.Id );
                                    checkinResult.JustCompletedAchievementAttempts = justCompletedAchievementAttemptsByPersonId?.GetValueOrNull( person.Person.Id );
                                    checkinResultList.Add( checkinResult );
                                }
                            }
                        }

                        if ( groupType.Labels != null && groupType.Labels.Any() )
                        {
                            printFromClient.AddRange( groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Client ) );
                            printFromServer.AddRange( groupType.Labels.Where( l => l.PrintFrom == Rock.Model.PrintFrom.Server ) );
                        }
                    }
                }
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "CheckinResultList", checkinResultList );
            mergeFields.Add( "Kiosk", CurrentCheckInState.Kiosk );
            mergeFields.Add( "RegistrationModeEnabled", CurrentCheckInState.Kiosk.RegistrationModeEnabled );
            mergeFields.Add( "Messages", CurrentCheckInState.Messages );
            if ( LocalDeviceConfig.CurrentGroupTypeIds != null )
            {
                var checkInAreas = LocalDeviceConfig.CurrentGroupTypeIds.Select( a => Rock.Web.Cache.GroupTypeCache.Get( a ) );
                mergeFields.Add( "CheckinAreas", checkInAreas );
            }

            if ( printFromClient.Any() )
            {
                var urlRoot = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority );

                /*
                // This is extremely useful when debugging with ngrok and an iPad on the local network.
                // X-Original-Host will contain the name of your ngrok hostname, therefore the labels will
                // get a LabelFile url that will actually work with that iPad.
                if ( Request.Headers["X-Original-Host" ] != null )
                {
                    var scheme = Request.Headers["X-Forwarded-Proto"] ?? "http";
                    urlRoot = string.Format( "{0}://{1}", scheme, Request.Headers.GetValues( "X-Original-Host" ).First() );
                }
                */

                printFromClient
                    .OrderBy( l => l.PersonId )
                    .ThenBy( l => l.Order )
                    .ToList()
                    .ForEach( l => l.LabelFile = urlRoot + l.LabelFile );

                AddLabelScript( printFromClient.ToJson() );
            }

            if ( printFromServer.Any() )
            {
                var messages = ZebraPrint.PrintLabels( printFromServer );
                mergeFields.Add( "ZebraPrintMessageList", messages );
            }

            if ( lbAnother.Visible )
            {
                var bodyTag = this.Page.Master.FindControl( "body" ) as HtmlGenericControl;
                if ( bodyTag != null )
                {
                    bodyTag.AddCssClass( "checkin-anotherperson" );
                }
            }

            //var successLavaTemplate = CurrentCheckInState.CheckInType.SuccessLavaTemplate;
            //lCheckinResultsHtml.Text = successLavaTemplate.ResolveMergeFields( mergeFields );

            GenerateQRCodes();

            RenderCheckinResults( checkinResultList );
        }

        /// <summary>
        /// Generates the qr codes.
        /// </summary>
        private void GenerateQRCodes()
        {
            if ( !LocalDeviceConfig.GenerateQRCodeForAttendanceSessions )
            {
                return;
            }

            HttpCookie attendanceSessionGuidsCookie = Request.Cookies[CheckInCookieKey.AttendanceSessionGuids];
            if ( attendanceSessionGuidsCookie == null )
            {
                attendanceSessionGuidsCookie = new HttpCookie( CheckInCookieKey.AttendanceSessionGuids );
                attendanceSessionGuidsCookie.Value = string.Empty;
            }

            // set (or reset) the expiration to be 8 hours from the current time)
            attendanceSessionGuidsCookie.Expires = RockDateTime.Now.AddHours( 8 );

            var attendanceSessionGuids = attendanceSessionGuidsCookie.Value.Split( ',' ).AsGuidList();
            attendanceSessionGuids = ValidAttendanceSessionGuids( attendanceSessionGuids );

            // Add the guid to the list of checkin session cookie guids if it's not already there.
            if ( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.HasValue &&
                !attendanceSessionGuids.Contains( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.Value ) )
            {
                attendanceSessionGuids.Add( CurrentCheckInState.CheckIn.CurrentFamily.AttendanceCheckinSessionGuid.Value );
            }

            attendanceSessionGuidsCookie.Value = attendanceSessionGuids.AsDelimited( "," );

            Rock.Web.UI.RockPage.AddOrUpdateCookie( attendanceSessionGuidsCookie );

            lCheckinQRCodeHtml.Text = string.Format( "<div class='qr-code-container text-center'><img class='img-responsive qr-code' src='{0}' alt='Check-in QR Code' width='500' height='500'></div>", GetAttendanceSessionsQrCodeImageUrl( attendanceSessionGuidsCookie ) );
        }

        /// <summary>
        /// Renders the checkin results and any achievements and celebrations
        /// </summary>
        /// <param name="checkinResultList">The checkin result list.</param>
        private void RenderCheckinResults( List<CheckinResult> checkinResultList )
        {
            List<PersonAchievementAttempt> personJustCompletedAchievementAttempts = new List<PersonAchievementAttempt>();

            foreach ( var checkinResult in checkinResultList.Where( a => a.JustCompletedAchievementAttempts != null ) )
            {
                foreach ( var achievementAttempt in checkinResult.JustCompletedAchievementAttempts )
                {
                    personJustCompletedAchievementAttempts.Add( new PersonAchievementAttempt( checkinResult.Person.Person, achievementAttempt ) );
                }
            }

            if ( personJustCompletedAchievementAttempts.Any() )
            {
                pnlAchievementSuccess.Visible = true;
                rptAchievementsSuccess.DataSource = personJustCompletedAchievementAttempts;
                rptAchievementsSuccess.DataBind();
            }

            pnlCheckinResults.Visible = true;
            rptCheckinResults.DataSource = checkinResultList;
            rptCheckinResults.DataBind();
        }

        /// <summary>
        /// 
        /// </summary>
        private class PersonAchievementAttempt
        {
            public PersonAchievementAttempt( Person person, AchievementAttempt achievementAttempt )
            {
                Person = person;
                AchievementAttempt = achievementAttempt;
            }

            public Person Person { get; }
            public AchievementAttempt AchievementAttempt { get; }
        }

        private Dictionary<string, object> GetAchievementMergeFields( AchievementAttempt achievementAttempt, Person person )
        {
            AchievementTypeCache achievementTypeCache = AchievementTypeCache.Get( achievementAttempt.AchievementTypeId );
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Person", person );
            mergeFields.Add( "Achievement", achievementAttempt );
            mergeFields.Add( "AchievementType", achievementTypeCache );
            mergeFields.Add( "NumberToAchieve", achievementTypeCache.NumberToAchieve );
            mergeFields.Add( "NumberToAccumulate", achievementTypeCache.NumberToAccumulate );
            mergeFields.Add( "ProgressCount", achievementTypeCache.GetProgressCount( achievementAttempt ) );
            mergeFields.Add( "ProgressPercent", ( achievementAttempt.Progress * 100 ) );

            return mergeFields;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptAchievementsSuccess control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAchievementsSuccess_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var personJustCompletedAchievementAttempt = e.Item.DataItem as PersonAchievementAttempt;
            if ( personJustCompletedAchievementAttempt == null )
            {
                return;
            }

            AchievementTypeCache achievementTypeCache = AchievementTypeCache.Get( personJustCompletedAchievementAttempt.AchievementAttempt.AchievementTypeId );
            var customSummaryLavaTemplate = achievementTypeCache.CustomSummaryLavaTemplate;

            if ( customSummaryLavaTemplate.IsNullOrWhiteSpace() )
            {
                customSummaryLavaTemplate = DebugSummaryTemplate;
            }


            var lAchievementSuccessHtml = e.Item.FindControl( "lAchievementSuccessHtml" ) as Literal;

            var mergeFields = GetAchievementMergeFields( personJustCompletedAchievementAttempt.AchievementAttempt, personJustCompletedAchievementAttempt.Person );
            lAchievementSuccessHtml.Text = customSummaryLavaTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCheckinResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCheckinResults_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var checkinResult = e.Item.DataItem as CheckinResult;
            if ( checkinResult == null )
            {
                return;
            }

            var lCheckinResultsPersonName = e.Item.FindControl( "lCheckinResultsPersonName" ) as Literal;
            var lCheckinResultsCheckinMessage = e.Item.FindControl( "lCheckinResultsCheckinMessage" ) as Literal;
            var pnlCheckinResultsCelebrationProgress = e.Item.FindControl( "pnlCheckinResultsCelebrationProgress" ) as Panel;

            lCheckinResultsPersonName.Text = checkinResult.Person.ToString();
            lCheckinResultsCheckinMessage.Text = $"{checkinResult.Group} in {checkinResult.Location.Name} at {checkinResult.Schedule}";


            if ( checkinResult.InProgressAchievementAttempts?.Any() == true )
            {
                List<PersonAchievementAttempt> inProgressPersonAchievementAttempts = new List<PersonAchievementAttempt>();

                foreach ( var achievementAttempt in checkinResult.InProgressAchievementAttempts )
                {
                    inProgressPersonAchievementAttempts.Add( new PersonAchievementAttempt( checkinResult.Person.Person, achievementAttempt ) );
                }

                pnlCheckinResultsCelebrationProgress.Visible = true;

                var rptCheckinResultsAchievementsProgress = e.Item.FindControl( "rptCheckinResultsAchievementsProgress" ) as Repeater;
                rptCheckinResultsAchievementsProgress.DataSource = inProgressPersonAchievementAttempts;
                rptCheckinResultsAchievementsProgress.DataBind();
            }
            else
            {
                pnlCheckinResultsCelebrationProgress.Visible = false;
            }
        }

        private const string DebugSummaryTemplate = @"
<pre>
Person: {{ Person.FullName }}
AchievementType: {{ AchievementType.Name }}
NumberToAchieve: {{ NumberToAchieve }}
NumberToAccumulate: {{ NumberToAccumulate }}
ProgressCount: {{ ProgressCount }}
ProgressPercent: {{ ProgressPercent }}%
</pre>
";

        /// <summary>
        /// Handles the ItemDataBound event of the rptCheckinResultsAchievementsProgress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCheckinResultsAchievementsProgress_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var lCheckinResultsAchievementProgressHtml = e.Item.FindControl( "lCheckinResultsAchievementProgressHtml" ) as Literal;
            PersonAchievementAttempt personAchievement = e.Item.DataItem as PersonAchievementAttempt;
            if ( personAchievement == null )
            {
                return;
            }

            var achievementAttempt = personAchievement.AchievementAttempt;

            var achievementType = AchievementTypeCache.Get( achievementAttempt.AchievementTypeId );
            if ( achievementType == null )
            {
                return;
            }

            var customSummaryLavaTemplate = achievementType.CustomSummaryLavaTemplate;
            var mergeFields = GetAchievementMergeFields( achievementAttempt, personAchievement.Person );

            if ( customSummaryLavaTemplate.IsNullOrWhiteSpace() )
            {
                customSummaryLavaTemplate = DebugSummaryTemplate;
            }

            lCheckinResultsAchievementProgressHtml.Text = customSummaryLavaTemplate?.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Handles the Click event of the lbDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDone_Click( object sender, EventArgs e )
        {
            NavigateToHomePage();
        }

        /// <summary>
        /// Handles the Click event of the lbAnother control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAnother_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                foreach ( var family in CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ) )
                {
                    foreach ( var person in family.People.Where( p => p.Selected ) )
                    {
                        person.Selected = false;

                        foreach ( var groupType in person.GroupTypes.Where( g => g.Selected ) )
                        {
                            groupType.Selected = false;
                        }
                    }
                }

                SaveState();
                NavigateToLinkedPage( AttributeKey.PersonSelectPage );

            }
            else
            {
                NavigateToHomePage();
            }
        }

        /// <summary>
        /// Checks the given list of the attendance check-in session guids are still valid
        /// and returns the valid ones back.
        /// NOTE: Because someone could check-in a person multiple times, only the
        /// latest attendance record will have the correct attendance check-in session guid.
        /// That means attendance check-in session guids could be old/invalid, so
        /// this method will filter out the old/ones so a QR code does not
        /// become unnecessarily dense.
        /// </summary>
        /// <param name="sessionGuids">The attendance session guids.</param>
        /// <returns></returns>
        private List<Guid> ValidAttendanceSessionGuids( List<Guid> sessionGuids )
        {
            if ( sessionGuids == null )
            {
                return new List<Guid>();
            }
            if ( !sessionGuids.Any() )
            {
                return sessionGuids;
            }

            return new AttendanceService( new RockContext() ).Queryable().AsNoTracking()
                .Where( a => sessionGuids.Contains( a.AttendanceCheckInSession.Guid ) )
                .Select( a => a.AttendanceCheckInSession.Guid ).Distinct().ToList();
        }

        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript( string jsonObject )
        {
            string script = string.Format( @"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/) && typeof window.RockCheckinNative === 'undefined') {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{
                printLabels();
            }}
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}

		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData),
            	function(result) {{
			        console.log('Tag printed');
			    }},
			    function(error) {{
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    alert('An error occurred while printing the labels. ' + error[0]);
			    }}
            );
	    }}
", jsonObject );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "addLabelScript", script, true );
        }


    }
}