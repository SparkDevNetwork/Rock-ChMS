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
using System.ComponentModel;
using System.Linq;
using System.Text;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Attendance Information about a person when listed in an attendance roster
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{FullName} {StatusString} {CheckInTime}" )]
    public class RosterAttendee
    {
        private Person _person;

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public Person Person => _person;

        /// <summary>
        /// Initializes a new instance of the <see cref="RosterAttendee"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public RosterAttendee( Rock.Model.Person person )
        {
            person.LoadAttributes();

            _person = person;
            if ( person.AgeClassification != AgeClassification.Adult )
            {
                ParentNames = Rock.Model.Person.GetFamilySalutation( person, finalSeparator: "and" );
            }

            HasHealthNote = GetHasHealthNote( person );
            HasLegalNote = GetHasLegalNote( person );
        }

        #region Properties

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId => _person.Id;

        /// <summary>
        /// Gets the person unique identifier.
        /// </summary>
        /// <value>
        /// The person unique identifier.
        /// </value>
        public Guid PersonGuid => _person.Guid;

        /// <summary>
        /// Gets the attendance ids.
        /// </summary>
        /// <value>
        /// The attendance ids.
        /// </value>
        public int[] AttendanceIds => Attendances.Select( a => a.Id ).ToArray();

        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
        /// </value>
        private List<Attendance> Attendances { get; set; } = new List<Attendance>();

        /// <summary>
        /// Gets the person's full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName => _person.FullName;

        /// <summary>
        /// Gets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName => _person.NickName;

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName => _person.LastName;

        /// <summary>
        /// Gets the parent names (if attendee is a child)
        /// </summary>
        /// <value>
        /// The parent names.
        /// </value>
        public string ParentNames { get; private set; }

        /// <summary>
        /// Gets the photo identifier.
        /// </summary>
        /// <value>
        /// The photo identifier.
        /// </value>
        public int? PhotoId => _person.PhotoId;

        /// <summary>
        /// Gets the age.
        /// </summary>
        /// <value>
        /// The age.
        /// </value>
        public int? Age => _person.Age;

        /// <summary>
        /// Gets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public Gender Gender => _person.Gender;

        /// <summary>
        /// If the person has a birthday within the next 6 days (including today),
        /// returns the person's birthday (abbreviated day of week),
        /// </summary>
        public string Birthday
        {
            get
            {
                if ( !IsBirthdayWeek )
                {
                    return null;
                }

                // If this Person's birthday is today, simply return "Today".
                int daysToBirthday = _person.DaysToBirthday;
                if ( daysToBirthday == 0 )
                {
                    return "Today";
                }

                return _person.BirthdayDayOfWeekShort;
            }
        }

        /// <summary>
        /// If the person has a birthday within the next 6 days (including today),
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is birthday week; otherwise, <c>false</c>.
        /// </value>
        public bool IsBirthdayWeek
        {
            get
            {
                return _person.DaysToBirthday < 7;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has health note.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has health note; otherwise, <c>false</c>.
        /// </value>
        public bool HasHealthNote { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has legal note.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has legal note; otherwise, <c>false</c>.
        /// </value>
        public bool HasLegalNote { get; private set; }

        /// <inheritdoc cref="Attendance.IsFirstTime"/>
        public bool IsFirstTime { get; private set; }

        /// <summary>
        /// Gets the unique tags.
        /// </summary>
        /// <value>
        /// The unique tags.
        /// </value>
        public List<string> UniqueTags { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the attendee's <seealso cref="AttendanceCode"/> tag
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag
        {
            get
            {
                return string.Join( ", ", UniqueTags );
            }
        }

        /// <summary>
        /// Gets the unique service times.
        /// </summary>
        /// <value>
        /// The unique service times.
        /// </value>
        public List<string> UniqueServiceTimes { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the service times.
        /// </summary>
        /// <value>
        /// The service times.
        /// </value>
        public string ServiceTimes
        {
            get
            {
                return string.Join( ", ", UniqueServiceTimes );
            }
        }

        /// <summary>
        /// Gets the current status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public RosterAttendeeStatus Status => GetCurrentStatus();

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        private RosterAttendeeStatus[] Statuses { get; set; }

        /// <summary>
        /// Gets a value indicating whether this attendee's room has Enable Presence;
        /// </summary>
        /// <value>
        ///   <c>true</c> if [room has enable presence]; otherwise, <c>false</c>.
        /// </value>
        public bool RoomHasEnablePresence { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this attendee's room has Allow Checkout
        /// </summary>
        /// <value>
        ///   <c>true</c> if [room has allow checkout]; otherwise, <c>false</c>.
        /// </value>
        public bool RoomHasAllowCheckout { get; private set; }

        /// <summary>
        /// Gets the status string.
        /// </summary>
        /// <value>
        /// The status string.
        /// </value>
        public string StatusString
        {
            get
            {
                return Status.GetDescription();
            }
        }

        /// <summary>
        /// Gets the check in time.
        /// </summary>
        /// <value>
        /// The check in time.
        /// </value>
        public DateTime CheckInTime { get; private set; }

        /// <summary>
        /// Gets the GroupTypeId (Checkin Area) of the group for the attendance
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public int? GroupTypeId { get; private set; }

        /// <summary>
        /// Gets the ScheduleId for the attendee's (Most Recent) occurrence
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int ScheduleId { get; private set; }

        /// <summary>
        /// The list of schedules that the person is currently attending.
        /// This is useful if a person is checked into multiple services.
        /// </summary>
        /// <value>
        /// The schedule ids.
        /// </value>
        public int[] ScheduleIds { get; private set; }

        /// <summary>
        /// Gets the name of the room (Occurrence Location.Name)
        /// </summary>
        /// <value>
        /// The name of the room.
        /// </value>
        public string RoomName { get; private set; }

        /// <summary>
        /// Gets the name of Checkin Group of the attendance
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; private set; }

        /// <summary>
        /// Gets the group type path ( Area1 > Area2 > Area51 )
        /// </summary>
        /// <value>
        /// The group type path.
        /// </value>
        public string GroupTypePath { get; private set; }

        #endregion Properties

        #region HTML

        /// <summary>
        /// Gets the person photo image tag.
        /// </summary>
        /// <returns></returns>
        public string GetPersonPhotoImageHtmlTag()
        {
            var imgTag = Rock.Model.Person.GetPersonPhotoImageTag( this._person, 50, 50, className: "avatar avatar-lg" );

            return imgTag;
        }

        /// <summary>
        /// Gets the status icon tag.
        /// </summary>
        /// <param name="isMobile">if set to <c>true</c> [is mobile].</param>
        /// <returns></returns>
        public string GetStatusIconHtmlTag( bool isMobile )
        {
            RosterAttendeeStatus currentStatus = GetCurrentStatus();

            var statusBuilder = new StringBuilder();
            foreach( var status in this.Statuses.Distinct())
            {
                statusBuilder.Append( GetStatusIconHtmlTagForStatus( isMobile, status ) );
            }    

            return statusBuilder.ToString() ;
        }

        /// <summary>
        /// Gets the status icon HTML tag for status.
        /// </summary>
        /// <param name="isMobile">if set to <c>true</c> [is mobile].</param>
        /// <param name="currentStatus">The current status.</param>
        /// <returns></returns>
        private string GetStatusIconHtmlTagForStatus( bool isMobile, RosterAttendeeStatus currentStatus )
        {
            string statusClass = string.Empty;
            string mobileIcon = string.Empty;

            switch ( currentStatus )
            {
                case RosterAttendeeStatus.CheckedIn:
                    statusClass = "warning";
                    mobileIcon = "&nbsp;";
                    break;
                case RosterAttendeeStatus.Present:
                    statusClass = "success";
                    mobileIcon = "<i class='fa fa-check'></i>";
                    break;
                case RosterAttendeeStatus.CheckedOut:
                    statusClass = "danger";
                    mobileIcon = "<i class='fa fa-minus'></i>";
                    break;
            }

            if ( isMobile )
            {
                return $"<span class='badge badge-circle badge-{statusClass}'>{mobileIcon}</span>";
            }
            else
            {
                return $"<span class='badge badge-{statusClass}'>{currentStatus.GetDescription()}</span>";
            }
        }

        /// <summary>
        /// Gets the current status.
        /// As determined in precendance 
        /// </summary>
        /// <returns></returns>
        private RosterAttendeeStatus GetCurrentStatus()
        {
            RosterAttendeeStatus matchingStatus;

            // The attendee might be in multiple statuses, pick the status that makes the most sense, which would be based on the precendance:
            // Present, Checked In, Checked Out
            if ( Statuses.Contains( RosterAttendeeStatus.Present ) )
            {
                matchingStatus = RosterAttendeeStatus.Present;
            }
            else if ( Statuses.Contains( RosterAttendeeStatus.CheckedIn ) )
            {
                matchingStatus = RosterAttendeeStatus.CheckedIn;
            }
            else if ( Statuses.Contains( RosterAttendeeStatus.CheckedOut ) )
            {
                matchingStatus = RosterAttendeeStatus.CheckedOut;
            }
            else
            {
                // shouldn't happen because RosterAttendeeStatus only has the above as possible values , but just in case
                matchingStatus = RosterAttendeeStatus.CheckedOut;
            }

            return matchingStatus;
        }

        /// <summary>
        /// Gets the attendee name HTML.
        /// </summary>
        /// <returns></returns>
        public string GetAttendeeNameHtml()
        {
            var result = $@"
<div class='name'>
    <span class='js-checkin-person-name'>{this.FullName}</span>
     <span class='badges d-sm-none d-print-inline'>{this.GetBadgesHtml( true )}</span>
</div>
<div class='parent-name small text-muted text-wrap'>{this.ParentNames}</div>";

            return result;
        }

        /// <summary>
        /// Gets the mobile tag and schedules HTML.
        /// </summary>
        /// <returns></returns>
        public string GetMobileTagAndSchedulesHtml()
        {
            return $"<div class='person-tag'>{this.Tag}</div><div class='small text-muted text-wrap'>{this.ServiceTimes}</div>";
        }

        /// <summary>
        /// Gets the badges markup.
        /// </summary>
        /// <param name="isMobile">if set to <c>true</c> [is mobile].</param>
        /// <returns></returns>
        public string GetBadgesHtml( bool isMobile )
        {
            var badgesSb = new StringBuilder();

            if ( this.IsBirthdayWeek )
            {
                if ( isMobile )
                {
                    badgesSb.Append( "&nbsp;<i class='fa fa-birthday-cake text-success'></i>" );
                }
                else
                {
                    badgesSb.Append( $"<div class='text-center text-success pull-left'><div><i class='fa fa-birthday-cake fa-2x'></i></div><div style='font-size: small;'>{this.Birthday}</div></div>" );
                }
            }

            var openDiv = isMobile ? string.Empty : "<div class='pull-left'>";
            var closeDiv = isMobile ? string.Empty : "</div>";
            var fa2x = isMobile ? string.Empty : " fa-2x";

            if ( this.HasHealthNote )
            {
                badgesSb.Append( $"{openDiv}&nbsp;<i class='fa fa-notes-medical{fa2x} text-danger' title='Health Note'></i>{openDiv}" );
            }

            if ( this.HasLegalNote )
            {
                badgesSb.Append( $"{openDiv}&nbsp;<i class='fa fa-clipboard{fa2x}' title='Legal Note'></i>{closeDiv}" );
            }

            if ( this.IsFirstTime )
            {
                badgesSb.Append( $"{openDiv}&nbsp;<i class='fa fa-star{fa2x} text-warning' title='First Time'></i>{closeDiv}" );
            }

            return badgesSb.ToString();
        }

        /// <summary>
        /// Gets the group name and path HTML. Group Name with GroupType Path underneath.
        /// </summary>
        /// <returns></returns>
        public string GetGroupNameAndPathHtml()
        {
            return $@"<div class='group-name'>{this.GroupName}</div> <div class='small text-muted text-wrap'>{this.GroupTypePath}</div>";
        }

        #endregion HTML

        #region Private methods

        /// <summary>
        /// Sets the attendance-specific properties.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <param name="checkinAreaPathsLookup">The checkin area paths lookup.</param>
        private void SetAttendanceInfo( Attendance attendance, Dictionary<int, CheckinAreaPath> checkinAreaPathsLookup )
        {
            // Keep track of each Attendance ID tied to this Attendee so we can manage them all as a group.
            this.Attendances.Add( attendance, true );

            // Tag(s).
            string tag = attendance.AttendanceCode?.Code;

            if ( tag.IsNotNullOrWhiteSpace() && !this.UniqueTags.Contains( tag, StringComparer.OrdinalIgnoreCase ) )
            {
                this.UniqueTags.Add( tag );
            }

            // Service Time(s).
            string serviceTime = attendance.Occurrence?.Schedule?.Name;

            if ( serviceTime.IsNotNullOrWhiteSpace() && !this.UniqueServiceTimes.Contains( serviceTime, StringComparer.OrdinalIgnoreCase ) )
            {
                this.UniqueServiceTimes.Add( serviceTime );
            }

            // Status: if this Attendee has multiple AttendanceOccurrences, the highest AttendeeStatus value among them wins.
            var latestAttendance = this.Attendances
                .OrderByDescending( a => a.StartDateTime )
                .First();

            this.Statuses = this.Attendances.Select( s => GetRosterAttendeeStatus( s.EndDateTime, s.PresentDateTime ) ).ToArray();

            // Check-in Time: if this Attendee has multiple AttendanceOccurrences, the latest StartDateTime value among them wins.
            this.CheckInTime = latestAttendance.StartDateTime;

            this.GroupTypeId = latestAttendance.Occurrence?.Group?.GroupTypeId;

            this.GroupName = latestAttendance.Occurrence?.Group?.Name;

            if ( GroupTypeId.HasValue )
            {
                this.GroupTypePath = checkinAreaPathsLookup.GetValueOrNull( GroupTypeId.Value )?.Path;
            }

            this.IsFirstTime = latestAttendance?.IsFirstTime ?? false;

            // ScheduleId should have a value, but just in case, we'll do some null safety.
            this.ScheduleId = latestAttendance.Occurrence?.ScheduleId ?? 0;

            this.ScheduleIds = this.Attendances.Select( a => a.Occurrence?.ScheduleId ?? 0 ).Distinct().ToArray();

            this.RoomName = latestAttendance.Occurrence?.Location?.Name;
        }

        /// <summary>
        /// Gets the roster attendee status.
        /// </summary>
        /// <param name="endDateTime">The <see cref="Attendance.EndDateTime"/></param>
        /// <param name="presentDateTime">The <see cref="Attendance.PresentDateTime"/></param>
        /// <returns></returns>
        public static RosterAttendeeStatus GetRosterAttendeeStatus( DateTime? endDateTime, DateTime? presentDateTime )
        {
            if ( endDateTime.HasValue )
            {
                return RosterAttendeeStatus.CheckedOut;
            }
            else if ( presentDateTime.HasValue )
            {
                return RosterAttendeeStatus.Present;
            }
            else
            {
                return RosterAttendeeStatus.CheckedIn;
            }
        }

        /// <summary>
        /// Determines if Attendance the meets roster status filter.
        /// </summary>
        /// <param name="attendance">The attendance.</param>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        /// <returns></returns>
        public static bool AttendanceMeetsRosterStatusFilter( Attendance attendance, RosterStatusFilter rosterStatusFilter )
        {
            RosterAttendeeStatus rosterAttendeeStatus = GetRosterAttendeeStatus( attendance.EndDateTime, attendance.PresentDateTime );

            return AppliesToRosterStatusFilter( rosterAttendeeStatus, rosterStatusFilter );
        }

        /// <summary>
        /// Returns true if any of this Attendee's current <see cref="Attendances"/> meets the specified RosterStatusFilter criteria
        /// </summary>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        /// <returns></returns>
        public bool MeetsRosterStatusFilter( RosterStatusFilter rosterStatusFilter )
        {
            return this.Statuses.Any( x => AppliesToRosterStatusFilter( x, rosterStatusFilter ) );
        }

        /// <summary>
        /// Returns true if the rosterAttendeeStatus meets the rosterStatusFilter
        /// </summary>
        /// <param name="rosterAttendeeStatus">The roster attendee status.</param>
        /// <param name="rosterStatusFilter">The roster status filter.</param>
        /// <returns></returns>
        private static bool AppliesToRosterStatusFilter( RosterAttendeeStatus rosterAttendeeStatus, RosterStatusFilter rosterStatusFilter )
        {
            switch ( rosterStatusFilter )
            {
                case RosterStatusFilter.CheckedIn:
                    return rosterAttendeeStatus == RosterAttendeeStatus.CheckedIn;
                case RosterStatusFilter.CheckedOut:
                    return rosterAttendeeStatus == RosterAttendeeStatus.CheckedOut;
                case RosterStatusFilter.Present:
                    return rosterAttendeeStatus == RosterAttendeeStatus.Present;
                case RosterStatusFilter.All:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether the person has a health note.
        /// </summary>
        /// <param name="person">The person.</param>
        private bool GetHasHealthNote( Rock.Model.Person person )
        {
            const string Person_Allergy = "Allergy";
            string attributeValue = person.GetAttributeValue( Person_Allergy );
            return attributeValue.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets whether the person has a legal note.
        /// </summary>
        /// <param name="person">The person.</param>
        private bool GetHasLegalNote( Rock.Model.Person person )
        {
            const string Person_LegalNotes = "LegalNotes";
            string attributeValue = person.GetAttributeValue( Person_LegalNotes );
            return attributeValue.IsNotNullOrWhiteSpace();
        }

        #endregion Private methods

        #region Static methods

        /// <summary>
        /// Returns a list of <see cref="RosterAttendee" /> from the attendance list.
        /// </summary>
        /// <param name="attendanceList">The attendance list.</param>
        /// <returns></returns>
        public static IList<RosterAttendee> GetFromAttendanceList( IList<Attendance> attendanceList )
        {
            return GetFromAttendanceList( attendanceList, null );
        }

        /// <summary>
        /// Returns a list of <see cref="RosterAttendee" /> from the attendance list,`
        /// With an option to specify the selected Checkin Area so that the Group Type Path logic can deal
        /// with situations where an area is part of more than one checkin area type.
        /// </summary>
        /// <param name="attendanceList">The attendance list.</param>
        /// <param name="selectedCheckinArea">The selected checkin area (or null for all areas).</param>
        /// <returns></returns>
        public static IList<RosterAttendee> GetFromAttendanceList( IList<Attendance> attendanceList, GroupTypeCache selectedCheckinArea )
        {
            var groupTypeIds = attendanceList.Select( a => a.Occurrence.Group.GroupTypeId ).Distinct();
            var groupTypes = groupTypeIds.Select( a => GroupTypeCache.Get( a ) ).Where( a => a != null );

            var groupTypeIdsWithAllowCheckout = groupTypes
                .Where( a => a.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT ).AsBoolean() )
                .Select( a => a.Id )
                .Distinct();

            var groupTypeIdsWithEnablePresence = groupTypes
                .Where( a => a.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean() )
                .Select( a => a.Id )
                .Distinct();

            Dictionary<int, CheckinAreaPath> checkinAreaPathsLookup;

            if ( selectedCheckinArea != null )
            {
                // If there is a checkin area filter, limit to group types within the selected check-in area.
                // this will help get the best path if a checkin area belongs to more than one checkin type
                checkinAreaPathsLookup = new GroupTypeService( new Rock.Data.RockContext() ).GetCheckinAreaDescendantsPath( selectedCheckinArea.Id )
                    .ToDictionary( k => k.GroupTypeId, v => v );
            }
            else
            {
                checkinAreaPathsLookup = new GroupTypeService( new Rock.Data.RockContext() ).GetAllCheckinAreaPaths()
                    .ToDictionary( k => k.GroupTypeId, v => v );
            }

            var attendees = new List<RosterAttendee>();
            foreach ( var attendance in attendanceList )
            {
                // Create an Attendee for each unique Person within the Attendance records.
                var person = attendance.PersonAlias.Person;

                RosterAttendee attendee = attendees.FirstOrDefault( a => a.PersonGuid == person.Guid );
                if ( attendee == null )
                {
                    attendee = new RosterAttendee( person );
                    attendees.Add( attendee );
                }

                attendee.RoomHasAllowCheckout = groupTypeIdsWithAllowCheckout.Contains( attendance.Occurrence.Group.GroupTypeId );
                attendee.RoomHasEnablePresence = groupTypeIdsWithEnablePresence.Contains( attendance.Occurrence.Group.GroupTypeId );

                // Add the attendance-specific property values.
                attendee.SetAttendanceInfo( attendance, checkinAreaPathsLookup );
            }

            return attendees;
        }

        #endregion Static methods
    }

    /// <summary>
    /// The status of an attendee.
    /// </summary>
    public enum RosterAttendeeStatus
    {
        /// <summary>
        /// Checked in
        /// </summary>
        [Description( "Checked-in" )]
        CheckedIn = 1,

        /// <summary>
        /// Present
        /// </summary>
        [Description( "Present" )]
        Present = 2,

        /// <summary>
        /// Checked out
        /// </summary>
        [Description( "Checked-out" )]
        CheckedOut = 3
    }
}