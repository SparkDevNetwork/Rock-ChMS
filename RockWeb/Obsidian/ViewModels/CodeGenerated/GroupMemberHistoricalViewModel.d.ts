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

export default interface GroupMemberHistorical extends Entity {
    id: number;
    archivedByPersonAliasId: number | null;
    archivedDateTime: RockDateType | null;
    attributes: Record<string, AttributeValue> | null;
    currentRowIndicator: boolean;
    effectiveDateTime: RockDateType;
    expireDateTime: RockDateType;
    groupId: number;
    groupMemberId: number;
    groupMemberStatus: number;
    groupRoleId: number;
    groupRoleName: string | null;
    inactiveDateTime: RockDateType | null;
    isArchived: boolean;
    isLeader: boolean;
    createdDateTime: RockDateType | null;
    modifiedDateTime: RockDateType | null;
    createdByPersonAliasId: number | null;
    modifiedByPersonAliasId: number | null;
    guid: Guid;
}
