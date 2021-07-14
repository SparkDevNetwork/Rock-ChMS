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

export default interface UserLogin extends Entity {
    id: number;
    apiKey: string | null;
    attributes: Record<string, AttributeValue> | null;
    entityTypeId: number | null;
    failedPasswordAttemptCount: number | null;
    failedPasswordAttemptWindowStartDateTime: RockDateType | null;
    isConfirmed: boolean | null;
    isLockedOut: boolean | null;
    isOnLine: boolean | null;
    isPasswordChangeRequired: boolean | null;
    lastActivityDateTime: RockDateType | null;
    lastLockedOutDateTime: RockDateType | null;
    lastLoginDateTime: RockDateType | null;
    lastPasswordChangedDateTime: RockDateType | null;
    lastPasswordExpirationWarningDateTime: RockDateType | null;
    password: string | null;
    personId: number | null;
    userName: string | null;
    createdDateTime: RockDateType | null;
    modifiedDateTime: RockDateType | null;
    createdByPersonAliasId: number | null;
    modifiedByPersonAliasId: number | null;
    guid: Guid;
}
