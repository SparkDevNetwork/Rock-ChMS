﻿// <copyright>
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
import { defineComponent, inject } from '../../Vendor/Vue/vue.js';
import PaneledBlockTemplate from '../../Templates/PaneledBlockTemplate.js';
import Loading from '../../Controls/Loading.js';
import { InvokeBlockActionFunc } from '../../Controls/RockBlock.js';
import { ConfigurationValues } from '../../Index.js';
import store from '../../Store/Index.js';
import { areEqual, Guid } from '../../Util/Guid.js';
import JavaScriptAnchor from '../../Elements/JavaScriptAnchor.js';
import RockForm from '../../Controls/RockForm.js';
import TextBox from '../../Elements/TextBox.js';
import RockButton from '../../Elements/RockButton.js';
import Person from '../../ViewModels/CodeGenerated/PersonViewModel.js';
import AttributeValue from '../../ViewModels/CodeGenerated/AttributeValueViewModel.js';
import AttributeValuesContainer from '../../Controls/AttributeValuesContainer.js';

export default defineComponent({
    name: 'Crm.AttributeValues',
    components: {
        PaneledBlockTemplate,
        Loading,
        JavaScriptAnchor,
        RockForm,
        TextBox,
        RockButton,
        AttributeValuesContainer
    },
    setup() {
        return {
            invokeBlockAction: inject('invokeBlockAction') as InvokeBlockActionFunc,
            configurationValues: inject('configurationValues') as ConfigurationValues
        };
    },
    data() {
        return {
            isLoading: false,
            isEditMode: false
        };
    },
    computed: {
        person(): Person | null {
            return (store.getters.personContext || null) as Person | null;
        },
        personGuid(): Guid | null {
            return this.person?.Guid || null;
        },
        categoryGuids(): Guid[] {
            return (this.configurationValues.CategoryGuids as Guid[] | null) || [];
        },
        useAbbreviatedNames(): boolean {
            return this.configurationValues.UseAbbreviatedNames as boolean;
        },
        attributeValues(): AttributeValue[] {
            const attributes = this.person?.Attributes || {};
            const attributeValues: AttributeValue[] = [];

            for (const key in attributes) {
                const attributeValue = attributes[key];
                const attribute = attributeValue.Attribute;

                if (this.categoryGuids && !attribute) {
                    continue;
                }

                if (this.categoryGuids && !attribute?.CategoryGuids.some(g1 => this.categoryGuids.some(g2 => areEqual(g1, g2)))) {
                    continue;
                }

                attributeValues.push(attributeValue);
            }

            attributeValues.sort((a, b) => {
                const aOrder = a.Attribute?.Order || 0;
                const bOrder = b.Attribute?.Order || 0;

                if (aOrder === bOrder) {
                    const aName = a.Attribute?.Name || '';
                    const bName = b.Attribute?.Name || '';

                    if (aName > bName) {
                        return 1;
                    }

                    if (aName < bName) {
                        return -1;
                    }
                }

                return aOrder - bOrder;
            });
            return attributeValues;
        }
    },
    methods: {
        goToViewMode() {
            this.isEditMode = false;
        },
        goToEditMode() {
            this.isEditMode = true;
        },
        async doSave(): Promise<void> {
            this.isLoading = true;

            const keyValueMap = {};

            for (const a of this.attributeValues) {
                if (a.Attribute) {
                    keyValueMap[a.Attribute.Key] = a.Value;
                }
            }

            await this.invokeBlockAction('SaveAttributeValues', {
                personGuid: this.personGuid,
                keyValueMap
            });

            this.goToViewMode();
            this.isLoading = false;
        }
    },
    template: `
<PaneledBlockTemplate class="panel-persondetails">
    <template v-slot:title>
        <i :class="configurationValues.BlockIconCssClass"></i>
        {{ configurationValues.BlockTitle }}
    </template>
    <template v-slot:titleAside>
        <div class="actions rollover-item pull-right">
            <JavaScriptAnchor title="Order Attributes" class="btn-link edit">
                <i class="fa fa-bars"></i>
            </JavaScriptAnchor>
            <JavaScriptAnchor title="Edit Attributes" class="btn-link edit" @click="goToEditMode">
                <i class="fa fa-pencil"></i>
            </JavaScriptAnchor>
        </div>
    </template>
    <template v-slot:default>
        <Loading :isLoading="isLoading">
            <AttributeValuesContainer v-if="!isEditMode" :attributeValues="attributeValues" :showEmptyValues="false" />
            <RockForm v-else @submit="doSave">
                <AttributeValuesContainer :attributeValues="attributeValues" isEditMode :showAbbreviatedName="useAbbreviatedNames" />
                <div class="actions">
                    <RockButton primary xs type="submit">Save</RockButton>
                    <RockButton link xs @click="goToViewMode">Cancel</RockButton>
                </div>
            </RockForm>
        </Loading>
    </template>
</PaneledBlockTemplate>`
});
