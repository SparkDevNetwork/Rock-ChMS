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
import AttributeValue from './AttributeValueViewModel';
import { RockDateType } from '../../Util/RockDate';
import { Guid } from '../../Util/Guid';

export default interface ConnectionRequest extends Entity {
    Id: number;
    AssignedGroupId: number | null;
    AssignedGroupMemberAttributeValues: string | null;
    AssignedGroupMemberRoleId: number | null;
    AssignedGroupMemberStatus: number | null;
    Attributes: Record<string, AttributeValue> | null;
    CampusId: number | null;
    Comments: string | null;
    ConnectionOpportunityId: number;
    ConnectionState: number;
    ConnectionStatusId: number;
    ConnectorPersonAliasId: number | null;
    FollowupDate: RockDateType | null;
    Order: number;
    PersonAliasId: number;
    CreatedDateTime: RockDateType | null;
    ModifiedDateTime: RockDateType | null;
    CreatedByPersonAliasId: number | null;
    ModifiedByPersonAliasId: number | null;
    Guid: Guid;
}