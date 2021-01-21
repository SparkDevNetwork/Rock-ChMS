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

export default interface Attribute extends Entity {
    Id: number;
    AbbreviatedName: string | null;
    AllowSearch: boolean;
    Attributes: Record<string, AttributeValue> | null;
    CategoryNames: (string | null)[];
    DefaultValue: string | null;
    Description: string | null;
    EnableHistory: boolean;
    EntityTypeId: number | null;
    EntityTypeQualifierColumn: string | null;
    EntityTypeQualifierValue: string | null;
    FieldTypeGuid: Guid;
    FieldTypeId: number;
    ForeignGuid: Guid | null;
    ForeignKey: string | null;
    IconCssClass: string | null;
    IsActive: boolean;
    IsAnalytic: boolean;
    IsAnalyticHistory: boolean;
    IsGridColumn: boolean;
    IsIndexEnabled: boolean;
    IsMultiValue: boolean;
    IsPublic: boolean;
    IsRequired: boolean;
    IsSystem: boolean;
    Key: string;
    Name: string;
    Order: number;
    PostHtml: string | null;
    PreHtml: string | null;
    ShowOnBulk: boolean;
    CreatedDateTime: string | Date | null;
    ModifiedDateTime: string | Date | null;
    CreatedByPersonAliasId: number | null;
    ModifiedByPersonAliasId: number | null;
    Guid: Guid;
    ForeignId: number | null;
}
