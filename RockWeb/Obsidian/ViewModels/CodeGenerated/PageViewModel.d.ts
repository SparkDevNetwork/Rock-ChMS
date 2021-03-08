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
import { RockDateType } from '../../Util/RockDate.js';
import { Guid } from '../../Util/Guid.js';

export default interface Page extends Entity {
    Id: number;
    AdditionalSettings: string | null;
    AllowIndexing: boolean;
    Attributes: Record<string, AttributeValue> | null;
    BodyCssClass: string | null;
    BreadCrumbDisplayIcon: boolean;
    BreadCrumbDisplayName: boolean;
    BrowserTitle: string | null;
    CacheControlHeaderSettings: string | null;
    Description: string | null;
    DisplayInNavWhen: number;
    EnableViewState: boolean;
    HeaderContent: string | null;
    IconBinaryFileId: number | null;
    IconCssClass: string | null;
    IncludeAdminFooter: boolean;
    InternalName: string | null;
    IsSystem: boolean;
    KeyWords: string | null;
    LayoutId: number;
    MedianPageLoadTimeDurationSeconds: number | null;
    MenuDisplayChildPages: boolean;
    MenuDisplayDescription: boolean;
    MenuDisplayIcon: boolean;
    Order: number;
    PageDisplayBreadCrumb: boolean;
    PageDisplayDescription: boolean;
    PageDisplayIcon: boolean;
    PageDisplayTitle: boolean;
    PageTitle: string | null;
    ParentPageId: number | null;
    RequiresEncryption: boolean;
    CreatedDateTime: RockDateType | null;
    ModifiedDateTime: RockDateType | null;
    CreatedByPersonAliasId: number | null;
    ModifiedByPersonAliasId: number | null;
    Guid: Guid;
}
