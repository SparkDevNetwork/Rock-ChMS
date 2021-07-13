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

import { defineComponent, inject } from 'vue';
import AttributeValuesContainer from '../../../Controls/AttributeValuesContainer';
import RockForm from '../../../Controls/RockForm';
import RockButton from '../../../Elements/RockButton';
import AttributeValue from '../../../ViewModels/CodeGenerated/AttributeValueViewModel';
import { RegistrationEntryState } from '../RegistrationEntry';

export default defineComponent( {
    name: 'Event.RegistrationEntry.RegistrationStart',
    components: {
        RockButton,
        AttributeValuesContainer,
        RockForm
    },
    setup()
    {
        return {
            registrationEntryState: inject( 'registrationEntryState' ) as RegistrationEntryState
        };
    },
    data()
    {
        return {
            attributeValues: [] as AttributeValue[]
        };
    },
    computed: {
        showPrevious(): boolean
        {
            return this.registrationEntryState.FirstStep === this.registrationEntryState.Steps.intro;
        }
    },
    methods: {
        onPrevious()
        {
            this.$emit( 'previous' );
        },
        onNext()
        {
            this.$emit( 'next' );
        }
    },
    watch: {
        viewModel: {
            immediate: true,
            handler()
            {
                this.attributeValues = this.registrationEntryState.ViewModel.registrationAttributesStart.map( a =>
                {
                    const currentValue = this.registrationEntryState.RegistrationFieldValues[ a.guid ] || '';

                    return {
                        attribute: a,
                        attributeId: a.id,
                        value: currentValue
                    } as AttributeValue;
                } );
            }
        },
        attributeValues: {
            immediate: true,
            deep: true,
            handler()
            {
                for ( const attributeValue of this.attributeValues )
                {
                    const attribute = attributeValue.attribute;

                    if ( attribute )
                    {
                        this.registrationEntryState.RegistrationFieldValues[ attribute.guid ] = attributeValue.value;
                    }
                }
            }
        }
    },
    template: `
<div class="registrationentry-registration-attributes">
    <RockForm @submit="onNext">
        <AttributeValuesContainer :attributeValues="attributeValues" isEditMode />

        <div class="actions row">
            <div class="col-xs-6">
                <RockButton v-if="showPrevious" btnType="default" @click="onPrevious">
                    Previous
                </RockButton>
            </div>
            <div class="col-xs-6 text-right">
                <RockButton btnType="primary" type="submit">
                    Next
                </RockButton>
            </div>
        </div>
    </RockForm>
</div>`
} );