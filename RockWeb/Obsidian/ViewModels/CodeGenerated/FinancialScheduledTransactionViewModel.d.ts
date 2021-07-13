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

export default interface FinancialScheduledTransaction extends Entity {
    id: number;
    attributes: Record<string, AttributeValue> | null;
    authorizedPersonAliasId: number;
    cardReminderDate: RockDateType | null;
    endDate: RockDateType | null;
    financialGatewayId: number | null;
    financialPaymentDetailId: number | null;
    foreignCurrencyCodeValueId: number | null;
    gatewayScheduleId: string | null;
    inactivateDateTime: RockDateType | null;
    isActive: boolean;
    lastRemindedDate: RockDateType | null;
    lastStatusUpdateDateTime: RockDateType | null;
    nextPaymentDate: RockDateType | null;
    numberOfPayments: number | null;
    sourceTypeValueId: number | null;
    startDate: RockDateType;
    status: number | null;
    statusMessage: string | null;
    summary: string | null;
    transactionCode: string | null;
    transactionFrequencyValueId: number;
    transactionTypeValueId: number | null;
    createdDateTime: RockDateType | null;
    modifiedDateTime: RockDateType | null;
    createdByPersonAliasId: number | null;
    modifiedByPersonAliasId: number | null;
    guid: Guid;
}
