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

import Entity from '../Entity.js';
import AttributeValue from './AttributeValueViewModel.js';
import { Guid } from '../../Util/Guid.js';

export default interface Person extends Entity {
    Id: number;
    AgeClassification: number;
    AnniversaryDate: string | Date | null;
    Attributes: Record<string, AttributeValue> | null;
    BirthDay: number | null;
    BirthMonth: number | null;
    BirthYear: number | null;
    CommunicationPreference: number;
    ConnectionStatusValueId: number | null;
    ContributionFinancialAccountId: number | null;
    DeceasedDate: string | Date | null;
    Email: string | null;
    EmailNote: string | null;
    EmailPreference: number;
    FirstName: string | null;
    FullName: string | null;
    Gender: number;
    GivingGroupId: number | null;
    GivingLeaderId: number;
    GraduationYear: number | null;
    InactiveReasonNote: string | null;
    IsDeceased: boolean;
    IsEmailActive: boolean;
    IsLockedAsChild: boolean;
    IsSystem: boolean;
    LastName: string | null;
    MaritalStatusValueId: number | null;
    MiddleName: string | null;
    NickName: string | null;
    PhotoId: number | null;
    PhotoUrl: string | null;
    PrimaryCampusId: number | null;
    PrimaryFamilyId: number | null;
    RecordStatusLastModifiedDateTime: string | Date | null;
    RecordStatusReasonValueId: number | null;
    RecordStatusValueId: number | null;
    RecordTypeValueId: number | null;
    ReviewReasonNote: string | null;
    ReviewReasonValueId: number | null;
    SuffixValueId: number | null;
    SystemNote: string | null;
    TitleValueId: number | null;
    TopSignalColor: string | null;
    TopSignalIconCssClass: string | null;
    TopSignalId: number | null;
    ViewedCount: number | null;
    CreatedDateTime: string | Date | null;
    ModifiedDateTime: string | Date | null;
    CreatedByPersonAliasId: number | null;
    ModifiedByPersonAliasId: number | null;
    Guid: Guid;
}
