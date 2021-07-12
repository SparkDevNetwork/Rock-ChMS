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

import Entity from '../Entity';
import { RockDateType } from '../../Util/RockDate';
import AttributeValue from './AttributeValueViewModel';
import { Guid } from '../../Util/Guid';

export default interface Group extends Entity {
    id: number;
    allowGuests: boolean | null;
    archivedByPersonAliasId: number | null;
    archivedDateTime: RockDateType | null;
    attendanceRecordRequiredForCheckIn: number;
    attributes: Record<string, AttributeValue> | null;
    campusId: number | null;
    description: string | null;
    disableScheduleToolboxAccess: boolean;
    disableScheduling: boolean;
    groupCapacity: number | null;
    groupSalutation: string | null;
    groupSalutationFull: string | null;
    groupTypeId: number;
    inactiveDateTime: RockDateType | null;
    inactiveReasonNote: string | null;
    inactiveReasonValueId: number | null;
    isActive: boolean;
    isArchived: boolean;
    isPublic: boolean;
    isSecurityRole: boolean;
    isSystem: boolean;
    name: string | null;
    order: number;
    parentGroupId: number | null;
    requiredSignatureDocumentTemplateId: number | null;
    rSVPReminderOffsetDays: number | null;
    rSVPReminderSystemCommunicationId: number | null;
    scheduleCancellationPersonAliasId: number | null;
    scheduleId: number | null;
    schedulingMustMeetRequirements: boolean;
    statusValueId: number | null;
    createdDateTime: RockDateType | null;
    modifiedDateTime: RockDateType | null;
    createdByPersonAliasId: number | null;
    modifiedByPersonAliasId: number | null;
    guid: Guid;
}
