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
using System.Collections.Generic;

namespace Rock.Client.Enums
{
    /// <summary>
    /// </summary>
    public enum AttendanceGraphBy
    {
        Total = 0x0,
        Group = 0x1,
        Campus = 0x2,
        Schedule = 0x3,
        Location = 0x4,
    }

    /// <summary>
    /// </summary>
    public enum AttendanceRule
    {
        None = 0x0,
        AddOnCheckIn = 0x1,
        AlreadyBelongs = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum AuditType
    {
        Add = 0x0,
        Modify = 0x1,
        Delete = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum AuthenticationServiceType
    {
        Internal = 0x0,
        External = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum BatchStatus
    {
        Pending = 0x0,
        Open = 0x1,
        Closed = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum BlockLocation
    {
        Layout = 0x0,
        Page = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum ColorDepth
    {
        BlackWhite = 0x0,
        Grayscale8bit = 0x1,
        Grayscale24bit = 0x2,
        Color8bit = 0x3,
        Color24bit = 0x4,
        Undefined = -1,
    }

    /// <summary>
    /// </summary>
    public enum CommunicationRecipientStatus
    {
        Pending = 0x0,
        Delivered = 0x1,
        Failed = 0x2,
        Cancelled = 0x3,
        Opened = 0x4,
        Sending = 0x5,
    }

    /// <summary>
    /// </summary>
    public enum CommunicationStatus
    {
        Transient = 0x0,
        Draft = 0x1,
        PendingApproval = 0x2,
        Approved = 0x3,
        Denied = 0x4,
    }

    /// <summary>
    /// </summary>
    [Flags]
    public enum ComparisonType
    {
        EqualTo = 0x1,
        NotEqualTo = 0x2,
        StartsWith = 0x4,
        Contains = 0x8,
        DoesNotContain = 0x10,
        IsBlank = 0x20,
        IsNotBlank = 0x40,
        GreaterThan = 0x80,
        GreaterThanOrEqualTo = 0x100,
        LessThan = 0x200,
        LessThanOrEqualTo = 0x400,
        EndsWith = 0x800,
        Between = 0x1000,
        RegularExpression = 0x2000,
    }

    /// <summary>
    /// </summary>
    public enum ContentChannelDateType
    {
        SingleDate = 0x1,
        DateRange = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum ContentChannelItemStatus
    {
        PendingApproval = 0x1,
        Approved = 0x2,
        Denied = 0x3,
    }

    /// <summary>
    /// </summary>
    public enum ContentControlType
    {
        CodeEditor = 0x0,
        HtmlEditor = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum DisplayInNavWhen
    {
        WhenAllowed = 0x0,
        Always = 0x1,
        Never = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum EmailPreference
    {
        EmailAllowed = 0x0,
        NoMassEmails = 0x1,
        DoNotEmail = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum FilterExpressionType
    {
        Filter = 0x0,
        GroupAll = 0x1,
        GroupAny = 0x2,
        GroupAllFalse = 0x3,
        GroupAnyFalse = 0x4,
    }

    /// <summary>
    /// </summary>
    public enum FollowingSuggestedStatus
    {
        PendingNotification = 0x0,
        Suggested = 0x1,
        Ignored = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum Format
    {
        JPG = 0x0,
        GIF = 0x1,
        PNG = 0x2,
        PDF = 0x3,
        Word = 0x4,
        Excel = 0x5,
        Text = 0x6,
        HTML = 0x7,
        Undefined = -1,
    }

    /// <summary>
    /// </summary>
    public enum Gender
    {
        Unknown = 0x0,
        Male = 0x1,
        Female = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum GroupCapacityRule
    {
        None = 0x0,
        Hard = 0x1,
        Soft = 0x2,
    }

    /// <summary>
    /// </summary>
    [Flags]
    public enum GroupLocationPickerMode
    {
        None = 0x0,
        Address = 0x1,
        Named = 0x2,
        Point = 0x4,
        Polygon = 0x8,
        GroupMember = 0x10,
        All = 0x1f,
    }

    /// <summary>
    /// </summary>
    public enum GroupMemberStatus
    {
        Inactive = 0x0,
        Active = 0x1,
        Pending = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum GroupMemberWorkflowTriggerType
    {
        MemberAddedToGroup = 0x0,
        MemberRemovedFromGroup = 0x1,
        MemberStatusChanged = 0x2,
        MemberRoleChanged = 0x3,
        MemberAttendedGroup = 0x4,
        MemberPlacedElsewhere = 0x5,
    }

    /// <summary>
    /// </summary>
    public enum JobNotificationStatus
    {
        All = 0x1,
        Success = 0x2,
        Error = 0x3,
        None = 0x4,
    }

    /// <summary>
    /// </summary>
    public enum MeetsGroupRequirement
    {
        Meets = 0x0,
        NotMet = 0x1,
        MeetsWithWarning = 0x2,
        NotApplicable = 0x3,
        Error = 0x4,
    }

    /// <summary>
    /// </summary>
    public enum MergeTemplateOwnership
    {
        Global = 0x0,
        Personal = 0x1,
        PersonalAndGlobal = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum MetricNumericDataType
    {
        Integer = 0x0,
        Decimal = 0x1,
        Currency = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum MetricValueType
    {
        Measure = 0x0,
        Goal = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum MICRStatus
    {
        Success = 0x0,
        Fail = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum NotificationClassification
    {
        Success = 0x0,
        Info = 0x1,
        Warning = 0x2,
        Danger = 0x3,
    }

    /// <summary>
    /// </summary>
    public enum PrintFrom
    {
        Client = 0x0,
        Server = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum PrintTo
    {
        Default = 0x0,
        Kiosk = 0x1,
        Location = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum RegistrantsSameFamily
    {
        No = 0x0,
        Yes = 0x1,
        Ask = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum RegistrationCostSummaryType
    {
        Cost = 0x0,
        Fee = 0x1,
        Discount = 0x2,
        Total = 0x3,
    }

    /// <summary>
    /// </summary>
    public enum RegistrationFeeType
    {
        Single = 0x0,
        Multiple = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum RegistrationFieldSource
    {
        PersonField = 0x0,
        PersonAttribute = 0x1,
        GroupMemberAttribute = 0x2,
        RegistrationAttribute = 0x4,
    }

    /// <summary>
    /// </summary>
    [Flags]
    public enum RegistrationNotify
    {
        None = 0x0,
        RegistrationContact = 0x1,
        GroupFollowers = 0x2,
        GroupLeaders = 0x4,
        All = 0x7,
    }

    /// <summary>
    /// </summary>
    public enum RegistrationPersonFieldType
    {
        FirstName = 0x0,
        LastName = 0x1,
        Campus = 0x2,
        Address = 0x3,
        Email = 0x4,
        Birthdate = 0x5,
        Gender = 0x6,
        MaritalStatus = 0x7,
        MobilePhone = 0x8,
        HomePhone = 0x9,
        WorkPhone = 0xa,
        Grade = 0xb,
    }

    /// <summary>
    /// </summary>
    public enum ReportFieldType
    {
        Property = 0x0,
        Attribute = 0x1,
        DataSelectComponent = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum RequirementCheckType
    {
        Sql = 0x0,
        Dataview = 0x1,
        Manual = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum Resolution
    {
        DPI72 = 0x0,
        DPI150 = 0x1,
        DPI300 = 0x2,
        DPI600 = 0x3,
        Undefined = -1,
    }

    /// <summary>
    /// </summary>
    public enum RSVP
    {
        No = 0x0,
        Yes = 0x1,
        Maybe = 0x2,
    }

    /// <summary>
    /// </summary>
    [Flags]
    public enum ScheduleType
    {
        None = 0x0,
        Weekly = 0x1,
        Custom = 0x2,
        Named = 0x4,
    }

    /// <summary>
    /// </summary>
    public enum SignatureDocumentAction
    {
        Email = 0x0,
        Embed = 0x1,
    }

    /// <summary>
    /// </summary>
    public enum SignatureDocumentStatus
    {
        None = 0x0,
        Sent = 0x1,
        Signed = 0x2,
        Cancelled = 0x3,
        Expired = 0x4,
    }

    /// <summary>
    /// </summary>
    public enum SpecialRole
    {
        None = 0x0,
        AllUsers = 0x1,
        AllAuthenticatedUsers = 0x2,
        AllUnAuthenticatedUsers = 0x3,
    }

    /// <summary>
    /// </summary>
    public enum TransactionGraphBy
    {
        Total = 0x0,
        FinancialAccount = 0x1,
        Campus = 0x2,
    }

    /// <summary>
    /// </summary>
    public enum WorkflowLoggingLevel
    {
        None = 0x0,
        Workflow = 0x1,
        Activity = 0x2,
        Action = 0x3,
    }

    /// <summary>
    /// </summary>
    public enum WorkflowTriggerType
    {
        PreSave = 0x0,
        PostSave = 0x1,
        PreDelete = 0x2,
        PostDelete = 0x3,
        ImmediatePostSave = 0x4,
    }

}
