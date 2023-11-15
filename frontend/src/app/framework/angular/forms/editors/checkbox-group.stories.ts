/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { CheckboxGroupComponent, SqxFrameworkModule } from '@app/framework/internal';

export default {
    title: 'Framework/CheckboxGroup',
    component: CheckboxGroupComponent,
    argTypes: {
        disabled: {
            control: 'boolean',
        },
        unsorted: {
            control: 'boolean',
        },
    },
    render: args => ({
        props: args,
        template: `
            <div style="padding: 2rem; max-width: 400px">
                <sqx-checkbox-group
                    [disabled]="disabled"
                    [layout]="layout"
                    (ngModelChange)="ngModelChange"
                    [ngModel]="model"
                    [unsorted]="unsorted"
                    [values]="values">
                </sqx-checkbox-group>
            </div>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                BrowserAnimationsModule,
                SqxFrameworkModule,
                SqxFrameworkModule.forRoot(),
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<CheckboxGroupComponent & { model: any }>;

export const Default: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
        model: [],
    },
};

export const Unsorted: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
        unsorted: true,
    },
};

export const Small: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor'],
        layout: 'Auto',
    },
};

export const SmallMultiline: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor'],
        layout: 'Multiline',
    },
};

export const Disabled: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
        disabled: true,
    },
};

export const Checked: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
        model: ['Lorem', 'ipsum'],
    },
};