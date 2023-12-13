/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { DropdownComponent, LocalizerService, RootViewComponent } from '@app/framework';

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
                (dropdownOpen)="load()">
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
    render: args => ({
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
    }),
    decorators: [
        moduleMetadata({
            declarations: [
                TestComponent,
            ],
            imports: [
                RootViewComponent,
            ],
            providers: [
                {
                    provide: LocalizerService,
                    useFactory: () => new LocalizerService(TRANSLATIONS),
                },
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<DropdownComponent & { model: any }>;

export const Default: Story = {
    args: {
        items: ['A', 'B', 'C'],
        model: 'B',
    },
};

export const WrongModel: Story = {
    args: {
        items: ['A', 'B', 'C'],
        model: 'D',
    },
};

export const Empty: Story = {
    args: {
        items: [],
        model: 'B',
    },
};

export const EmptyLoading: Story = {
    args: {
        items: [],
        itemsLoading: true,
    },
};

export const NoSearch: Story = {
    args: {
        items: ['A', 'B', 'C'],
        canSearch: false,
    },
};

export const FullWidth: Story = {
    args: {
        items: ['A', 'B', 'C'],
        dropdownFullWidth: true,
    },
};

export const Lazy: Story = {
    render: args => ({
        props: args,
        template: `
            <sqx-dropdown-test></sqx-dropdown-test>
        `,
    }),
};

export const ComplexValues: Story = {
    render: args => ({
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
    }),
    args: {
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
    },
};