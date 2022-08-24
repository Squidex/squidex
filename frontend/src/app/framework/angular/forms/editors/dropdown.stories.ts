/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { DropdownComponent, LocalizerService, SqxFrameworkModule } from '@app/framework';

const TRANSLATIONS = {
    'common.search': 'Search',
    'common.empty': 'Nothing available.',
};

@Component({
    selector: 'sqx-dropdown-test',
    template: `
        <sqx-root-view>
            <sqx-dropdown 
                [allowOpen]="true" 
                [items]="items"
                [itemsLoading]="itemsLoading"
                (open)="load()">
            </sqx-dropdown>
        </sqx-root-view>
    `,
})
class TestComponent {
    public items: string[] = [];
    public itemsLoading = false;

    public load() {
        this.items = [];
        this.itemsLoading = true;

        setTimeout(() => {
            this.items = ['A', 'B'];
            this.itemsLoading = false;
        }, 1000);
    }
}

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
            declarations: [
                TestComponent,
            ],
            imports: [
                BrowserAnimationsModule,
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
                [itemsLoading]="itemsLoading"
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
                [disabled]="disabled"
                [dropdownPosition]="'bottom-left'"
                [dropdownFullWidth]="dropdownFullWidth"
                [items]="items"
                [itemsLoading]="itemsLoading"
                [searchProperty]="searchProperty"
                [ngModel]="model"
                [valueProperty]="valueProperty">
                <ng-template let-target="$implicit">
                    {{target.label}}
                </ng-template>
            </sqx-dropdown>
        </sqx-root-view>
    `,
});

const Template3: Story<DropdownComponent & { model: any }> = (args: DropdownComponent) => ({
    props: args,
    template: `
        <sqx-dropdown-test></sqx-dropdown-test>
    `,
});

export const Default = Template.bind({});

Default.args = {
    items: ['A', 'B', 'C'],
    model: 'B',
};

export const WrongModel = Template.bind({});

WrongModel.args = {
    items: ['A', 'B', 'C'],
    model: 'D',
};

export const Empty = Template.bind({});

Empty.args = {
    items: [],
    model: 'B',
};

export const EmptyLoading = Template.bind({});

EmptyLoading.args = {
    items: [],
    itemsLoading: true,
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

export const Lazy = Template3.bind({});