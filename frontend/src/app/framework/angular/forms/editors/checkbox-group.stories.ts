/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { CheckboxGroupComponent, SqxFrameworkModule } from '@app/framework';

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

const Template: Story<CheckboxGroupComponent & { model: any }> = (args: CheckboxGroupComponent) => ({
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
});

export const Default = Template.bind({});

Default.args = {
    values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
    model: [],
};

export const Unsorted = Template.bind({});

Unsorted.args = {
    values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
    unsorted: true,
};

export const Small = Template.bind({});

Small.args = {
    values: ['Lorem', 'ipsum', 'dolor'],
    layout: 'Auto',
};

export const SmallMultiline = Template.bind({});

SmallMultiline.args = {
    values: ['Lorem', 'ipsum', 'dolor'],
    layout: 'Multiline',
};

export const Disabled = Template.bind({});

Disabled.args = {
    values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
    disabled: true,
};

export const Checked = Template.bind({});

Checked.args = {
    values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
    model: ['Lorem', 'ipsum'],
};