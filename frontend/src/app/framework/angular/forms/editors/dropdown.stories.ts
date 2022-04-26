/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { DropdownComponent, LocalizerService, SqxFrameworkModule } from '@app/framework';

const TRANSLATIONS = {
    'common.search': 'Search',
};

export default {
    title: 'Framework/Dropdown',
    component: DropdownComponent,
    argTypes: {
        disabled: {
            control: 'boolean',
        },
        dropdownFullWidth: {
            control: 'boolean',
        },
    },
    decorators: [
        moduleMetadata({
            imports: [
                SqxFrameworkModule,
                SqxFrameworkModule.forRoot(),
            ],
            providers: [
                { provide: LocalizerService, useFactory: () => new LocalizerService(TRANSLATIONS) },
            ],
        }),
    ],
} as Meta;

const Template: Story<DropdownComponent & { model: any }> = (args: DropdownComponent) => ({
    props: args,
    template: `
        <sqx-root-view>
            <sqx-dropdown 
                [disabled]="disabled"
                [dropdownPosition]="'bottom-left'"
                [dropdownFullWidth]="dropdownFullWidth"
                [items]="items"
                [ngModel]="model">
            </sqx-dropdown>
        </sqx-root-view>
    `,
});

const Template2: Story<DropdownComponent & { model: any }> = (args: DropdownComponent) => ({
    props: args,
    template: `
        <sqx-root-view>
            <sqx-dropdown
                [searchProperty]="searchProperty"
                [disabled]="disabled"
                [dropdownPosition]="'bottom-left'"
                [dropdownFullWidth]="dropdownFullWidth"
                [items]="items"
                [ngModel]="model"
                [valueProperty]="valueProperty">
                <ng-template let-target="$implicit">
                    {{target.label}}
                </ng-template>
            </sqx-dropdown>
        </sqx-root-view>
    `,
});

export const Default = Template.bind({});

Default.args = {
    items: ['A', 'B', 'C'],
    model: 'B',
};

export const NoSearch = Template.bind({});

NoSearch.args = {
    items: ['A', 'B', 'C'],
    canSearch: false,
};

export const FullWidth = Template.bind({});

FullWidth.args = {
    items: ['A', 'B', 'C'],
    dropdownFullWidth: true,
};

export const ComplexValues = Template2.bind({});

ComplexValues.args = {
    searchProperty: 'label',
    items: [{
        id: 1,
        label: 'Lorem',
    }, {
        id: 2,
        label: 'ipsum',
    }, {
        id: 3,
        label: 'dolor',
    }, {
        id: 4,
        label: 'sit',
    }],
    model: 2,
    valueProperty: 'id',
};