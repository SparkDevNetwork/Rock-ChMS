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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.Workflow.Action.CheckIn
{
    /// <summary>
    /// </summary>
    [ActionCategory( "Check-In" )]
    [Description( "Saves the selected check-in data as attendance" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Save Attendance" )]

    [BooleanField(
        "Enforce Strict Location Threshold",
        Key = AttributeKey.EnforceStrictLocationThreshold,
        Description = "If enabled, the room capacity will be re-checked prior to saving attendance to reduce the possibility surpassing the room’s capacity threshold.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [CodeEditorField( "Not Checked-In Message Format",
        Key = AttributeKey.NotCheckedInMessageFormat,
        Description = " <span class='tip tip-lava'></span>",
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
         DefaultValue = @"
{{ Person.FullName }} was not able to be checked into {{ Group.Name }} in {{ Location.Name }} at {{ Schedule.Name }} due to a capacity limit. Please re-check {{ Person.NickName }} in to a new room.",
        Order = 1 )]
    public class SaveAttendance : CheckInActionComponent
    {
        private static class AttributeKey
        {
            public const string EnforceStrictLocationThreshold = "EnforceStrictLocationThreshold";
            public const string NotCheckedInMessageFormat = "NotChecked-InMessageFormat";
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            Stopwatch stopwatchSaveAttendance = Stopwatch.StartNew();
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState == null )
            {
                return false;
            }

            AttendanceCode attendanceCode = null;

            bool reuseCodeForFamily = checkInState.CheckInType != null && checkInState.CheckInType.ReuseSameCode;
            int securityCodeAlphaNumericLength = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeAlphaNumericLength : 3;
            int securityCodeAlphaLength = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeAlphaLength : 0;
            int securityCodeNumericLength = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeNumericLength : 0;
            bool securityCodeNumericRandom = checkInState.CheckInType != null ? checkInState.CheckInType.SecurityCodeNumericRandom : true;

            bool enablePresence = checkInState.CheckInType != null && checkInState.CheckInType.EnablePresence;

            var attendanceCodeService = new AttendanceCodeService( rockContext );
            var attendanceService = new AttendanceService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var attendanceRecords = new List<Attendance>();

            AttendanceCheckInSession attendanceCheckInSession = new AttendanceCheckInSession()
            {
                DeviceId = checkInState.DeviceId,
                ClientIpAddress = RockPage.GetClientIpAddress()
            };

            checkInState.Messages.Clear();

            var family = checkInState.CheckIn.CurrentFamily;
            if ( family != null )
            {
                var currentOccurrences = new List<OccurrenceRecord>();
                foreach ( var person in family.GetPeople( true ) )
                {
                    if ( reuseCodeForFamily && attendanceCode != null )
                    {
                        person.SecurityCode = attendanceCode.Code;
                    }
                    else
                    {
                        attendanceCode = AttendanceCodeService.GetNew( securityCodeAlphaNumericLength, securityCodeAlphaLength, securityCodeNumericLength, securityCodeNumericRandom );
                        person.SecurityCode = attendanceCode.Code;
                    }

                    foreach ( var groupType in person.GetGroupTypes( true ) )
                    {
                        foreach ( var group in groupType.GetGroups( true ) )
                        {
                            if ( groupType.GroupType.AttendanceRule == AttendanceRule.AddOnCheckIn &&
                                groupType.GroupType.DefaultGroupRoleId.HasValue &&
                                !groupMemberService.GetByGroupIdAndPersonId( group.Group.Id, person.Person.Id, true ).Any() )
                            {
                                var groupMember = new GroupMember();
                                groupMember.GroupId = group.Group.Id;
                                groupMember.PersonId = person.Person.Id;
                                groupMember.GroupRoleId = groupType.GroupType.DefaultGroupRoleId.Value;
                                groupMemberService.Add( groupMember );
                            }

                            foreach ( var location in group.GetLocations( true ) )
                            {
                                bool isCheckedIntoLocation = false;
                                foreach ( var schedule in location.GetSchedules( true ) )
                                {
                                    var startDateTime = schedule.CampusCurrentDateTime;

                                    // If we're enforcing strict location thresholds, then before we create an attendance record
                                    // we need to check the location-schedule's current count.
                                    if ( GetAttributeValue( action, AttributeKey.EnforceStrictLocationThreshold ).AsBoolean() && location.Location.SoftRoomThreshold.HasValue )
                                    {
                                        EnforceStrictLocationThreshold( action, checkInState, attendanceService, currentOccurrences, person, group, location, schedule, startDateTime );
                                    }

                                    // Only create one attendance record per day for each person/schedule/group/location
                                    var attendance = attendanceService.Get( startDateTime, location.Location.Id, schedule.Schedule.Id, group.Group.Id, person.Person.Id );
                                    if ( attendance == null )
                                    {
                                        var primaryAlias = personAliasService.GetPrimaryAlias( person.Person.Id );
                                        if ( primaryAlias != null )
                                        {
                                            attendance = attendanceService.AddOrUpdate( primaryAlias.Id, startDateTime.Date, group.Group.Id,
                                                location.Location.Id, schedule.Schedule.Id, location.CampusId,
                                                checkInState.Kiosk.Device.Id, checkInState.CheckIn.SearchType?.Id,
                                                checkInState.CheckIn.SearchValue, family.Group.Id, attendanceCode.Id );

                                            attendance.PersonAlias = primaryAlias;
                                        }
                                    }

                                    attendance.AttendanceCheckInSession = attendanceCheckInSession;

                                    attendance.DeviceId = checkInState.Kiosk.Device.Id;
                                    attendance.SearchTypeValueId = checkInState.CheckIn.SearchType?.Id;
                                    attendance.SearchValue = checkInState.CheckIn.SearchValue;
                                    attendance.CheckedInByPersonAliasId = checkInState.CheckIn.CheckedInByPersonAliasId;
                                    attendance.SearchResultGroupId = family.Group.Id;
                                    attendance.AttendanceCodeId = attendanceCode.Id;
                                    attendance.StartDateTime = startDateTime;
                                    attendance.EndDateTime = null;
                                    attendance.CheckedOutByPersonAliasId = null;
                                    attendance.DidAttend = true;
                                    attendance.Note = group.Notes;
                                    attendance.IsFirstTime = person.FirstTime;

                                    /*
                                        7/16/2020 - JH
                                        If EnablePresence is true for this Check-in configuration, it will be the responsibility of the room
                                        attendants to mark a given Person as present, so do not set the 'Present..' property values below.
                                        Otherwise, set the values to match those of the Check-in values: the Person checking them in will
                                        have simultaneously marked them as present.

                                        Also, note that we sometimes reuse Attendance records (i.e. the Person was already checked into this
                                        schedule/group/location, might have already been checked out, and also might have been previously
                                        marked as present). In this case, the same 'Present..' rules apply, but we might need to go so far
                                        as to null-out the previously set 'Present..' property values, hence the conditional operators below.
                                    */
                                    attendance.PresentDateTime = enablePresence ? ( DateTime? ) null : startDateTime;
                                    attendance.PresentByPersonAliasId = enablePresence ? null : checkInState.CheckIn.CheckedInByPersonAliasId;

                                    KioskLocationAttendance.AddAttendance( attendance );
                                    isCheckedIntoLocation = true;

                                    // Keep track of attendance (Ids) for use by other actions later in the workflow pipeline
                                    attendanceRecords.Add( attendance );
                                }

                                // If the person was NOT checked into the location for any schedule then remove the location
                                if ( !isCheckedIntoLocation )
                                {
                                    group.Locations.Remove( location );
                                }
                            }
                        }
                    }
                }
            }

            if ( checkInState.CheckInType.AchievementTypes.Any() )
            {
                // Get any achievements that were in-progress *prior* to adding these attendance records
                var configuredAchievementTypeIds = checkInState.CheckInType.AchievementTypes.Select( a => a.Id ).ToList();
                var inProgressAchievementAttemptsPriorToSaveChangesByPersonId = GetInProgressAchievementAttemptsByPersonId( rockContext, attendanceRecords, configuredAchievementTypeIds );

                SaveChangesResult saveChangesResult = rockContext.SaveChanges( new SaveChangesArgs { IsAchievementsEnabled = true } );

                // calculate the "Just Completed" achievements as-of *after* save changes
                checkInState.CheckIn.JustCompletedAchievementAttemptsByPersonId = GetJustCompletedAchievementAttemptsByPersonId( configuredAchievementTypeIds, inProgressAchievementAttemptsPriorToSaveChangesByPersonId, saveChangesResult );

                // get any InProgress attempts as-of after save changes
                checkInState.CheckIn.InProgressAchievementAttemptsByPersonId = GetInProgressAchievementAttemptsByPersonId( rockContext, attendanceRecords, configuredAchievementTypeIds );
            }
            else
            {
                rockContext.SaveChanges();
            }

            // Now that the records are persisted, take the Ids and save them to the temp CheckInFamliy object
            family.AttendanceIds = attendanceRecords.Select( a => a.Id ).ToList();
            family.AttendanceCheckinSessionGuid = attendanceCheckInSession.Guid;
            attendanceRecords = null;

            Debug.WriteLine( $"{stopwatchSaveAttendance.Elapsed.TotalMilliseconds}ms  SaveAttendance" );

            return true;
        }

        /// <summary>
        /// Gets the just completed achievement attempts by person identifier.
        /// </summary>
        /// <param name="configuredAchievementTypeIds">The configured achievement type ids.</param>
        /// <param name="inProgressAchievementAttemptsPriorToSaveChangesByPersonId">The in progress achievement attempts prior to save changes by person identifier.</param>
        /// <param name="saveChangesResult">The save changes result.</param>
        /// <returns></returns>
        private static Dictionary<int, AchievementAttempt[]> GetJustCompletedAchievementAttemptsByPersonId( List<int> configuredAchievementTypeIds, Dictionary<int, AchievementAttempt[]> inProgressAchievementAttemptsPriorToSaveChangesByPersonId, SaveChangesResult saveChangesResult )
        {
            var inProgressAchievementIdsPriorToSaveChanges = inProgressAchievementAttemptsPriorToSaveChangesByPersonId.SelectMany( a => a.Value.Select( x => x.Id ) ).ToArray();

            // AchievementAttempts that were updated/added as a result of these attendances
            var saveChangesAchievementAttempts = saveChangesResult.AchievementAttempts.Where( a => configuredAchievementTypeIds.Contains( a.AchievementTypeId ) );
            var updatedAchievementAttempts = saveChangesAchievementAttempts.Where( a => inProgressAchievementIdsPriorToSaveChanges.Contains( a.Id ) ).ToArray();
            var addedAchievementAttempts = saveChangesAchievementAttempts.Where( a => !inProgressAchievementIdsPriorToSaveChanges.Contains( a.Id ) ).ToArray();

            // determine the "Just Completed" attempts by seeing what achievements changed from InProgress To IsSuccessful
            var justCompletedAchievementAttempts = updatedAchievementAttempts
                .Where( a => a.IsSuccessful && inProgressAchievementIdsPriorToSaveChanges.Contains( a.Id ) )
                .ToList();

            // Add any new achievements that were added as a result of SaveChanges, and include any "IsSuccessful" ones as "Just Completed"
            justCompletedAchievementAttempts.AddRange( addedAchievementAttempts.Where( a => a.IsSuccessful ) );

            var justCompletedAchievementAttemptsByPersonId = inProgressAchievementAttemptsPriorToSaveChangesByPersonId.Select( a => new
            {
                PersonId = a.Key,
                JustCompletedAchievementAttempts = a.Value.Where( x => justCompletedAchievementAttempts.Any( c => c.Id == x.Id ) ).ToArray()
            } ).ToDictionary( k => k.PersonId, v => v.JustCompletedAchievementAttempts );

            return justCompletedAchievementAttemptsByPersonId;
        }

        /// <summary>
        /// Gets the in progress achievement attempts by person identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attendanceRecords">The attendance records.</param>
        /// <param name="configuredAchievementTypeIds">The configured achievement type ids.</param>
        /// <returns></returns>
        private static Dictionary<int, AchievementAttempt[]> GetInProgressAchievementAttemptsByPersonId( RockContext rockContext, List<Attendance> attendanceRecords, List<int> configuredAchievementTypeIds )
        {
            var achievementAttemptService = new AchievementAttemptService( rockContext );
            Dictionary<int, AchievementAttempt[]> inProgressAchievementAttemptsByPersonId = new Dictionary<int, AchievementAttempt[]>();
            foreach ( var attendance in attendanceRecords.Where( a => a.PersonAliasId.HasValue ) )
            {
                var attendancePersonAlias = attendance.PersonAlias ?? new PersonAliasService( rockContext ).Get( attendance.PersonAliasId.Value );
                var personId = attendancePersonAlias?.PersonId ?? 0;
                var personInProgressAchievementAttempts = achievementAttemptService
                    .QueryByPersonId( personId )
                    .Where( a => configuredAchievementTypeIds.Contains( a.AchievementTypeId ) && !a.IsSuccessful && !a.IsClosed )
                    .AsNoTracking()
                    .ToArray();

                inProgressAchievementAttemptsByPersonId.AddOrIgnore( personId, personInProgressAchievementAttempts );
            }

            return inProgressAchievementAttemptsByPersonId;
        }

        /// <summary>
        /// Gets the current occurrence from the given list for the matching location, schedule and startDateTime.
        /// </summary>
        /// <param name="currentOccurrences">The current occurrences.</param>
        /// <param name="location">The location.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <returns></returns>
        private OccurrenceRecord GetCurrentOccurrence( List<OccurrenceRecord> currentOccurrences, CheckInLocation location, CheckInSchedule schedule, DateTime startDateTime )
        {
            return currentOccurrences
                    .Where( a =>
                        a.Date == startDateTime.Date
                        && a.LocationId == location.Location.Id
                        && a.ScheduleId == schedule.Schedule.Id )
                    .FirstOrDefault();
        }

        /// <summary>
        /// Enforces the strict location threshold by removing attendances that would have ended up going into full location+schedules.
        /// Note: The is also checked earlier in the check-in process, so this catches ones that might have just gotten full in the last few seconds.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="checkInState">State of the check in.</param>
        /// <param name="attendanceService">The attendance service.</param>
        /// <param name="currentOccurrences">The current occurrences.</param>
        /// <param name="person">The person.</param>
        /// <param name="group">The group.</param>
        /// <param name="location">The location.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="startDateTime">The start date time.</param>
        private void EnforceStrictLocationThreshold( WorkflowAction action, CheckInState checkInState, AttendanceService attendanceService, List<OccurrenceRecord> currentOccurrences, CheckInPerson person, CheckInGroup group, CheckInLocation location, CheckInSchedule schedule, DateTime startDateTime )
        {
            var thresHold = location.Location.SoftRoomThreshold.Value;
            if ( checkInState.ManagerLoggedIn && location.Location.FirmRoomThreshold.HasValue && location.Location.FirmRoomThreshold.Value > location.Location.SoftRoomThreshold.Value )
            {
                thresHold = location.Location.FirmRoomThreshold.Value;
            }

            var currentOccurrence = GetCurrentOccurrence( currentOccurrences, location, schedule, startDateTime.Date );

            // The totalAttended is the number of people still checked in (not people who have been checked-out)
            // not counting the current person who may already be checked in,
            // + the number of people we have checked in so far (but haven't been saved yet).
            var attendanceQry = attendanceService.GetByDateOnLocationAndSchedule( startDateTime.Date, location.Location.Id, schedule.Schedule.Id )
                .AsNoTracking()
                .Where( a => a.EndDateTime == null );

            // Only process if the current person is NOT already checked-in to this location and schedule
            if ( !attendanceQry.Where( a => a.PersonAlias.PersonId == person.Person.Id ).Any() )
            {
                var totalAttended = attendanceQry.Count() + ( currentOccurrence == null ? 0 : currentOccurrence.Count );

                // If over capacity, remove the schedule and add a warning message.
                if ( totalAttended >= thresHold )
                {
                    // Remove the schedule since the location was full for this schedule.  
                    location.Schedules.Remove( schedule );

                    var message = new CheckInMessage()
                    {
                        MessageType = MessageType.Warning
                    };

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Person", person.Person );
                    mergeFields.Add( "Group", group.Group );
                    mergeFields.Add( "Location", location.Location );
                    mergeFields.Add( "Schedule", schedule.Schedule );
                    message.MessageText = GetAttributeValue( action, AttributeKey.NotCheckedInMessageFormat ).ResolveMergeFields( mergeFields );

                    // Now add it to the check-in state message list for others to see.
                    checkInState.Messages.Add( message );
                    return;
                }
                else
                {
                    // Keep track of anyone who was checked in so far.
                    if ( currentOccurrence == null )
                    {
                        currentOccurrence = new OccurrenceRecord()
                        {
                            Date = startDateTime.Date,
                            LocationId = location.Location.Id,
                            ScheduleId = schedule.Schedule.Id
                        };

                        currentOccurrences.Add( currentOccurrence );
                    }

                    currentOccurrence.Count += 1;
                }
            }
        }
    }

    class OccurrenceRecord
    {
        public int ScheduleId { get; set; }
        public int LocationId { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }
}