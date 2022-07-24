/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { RadioGroupComponent, SqxFrameworkModule } from '@app/framework';

export default {
    title: 'Framework/RadioGroup',
    component: RadioGroupComponent,
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

const Template: Story<RadioGroupComponent & { model: any }> = (args: RadioGroupComponent) => ({
    props: args,
    template: `
        <div style="padding: 2rem; max-width: 400px">
            <sqx-radio-group
                [disabled]="disabled"
                [layout]="layout"
                (ngModelChange)="ngModelChange"
                [ngModel]="model"
                [unsorted]="unsorted"
                [values]="values">
            </sqx-radio-group>
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
    unsorted: false,
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
    model: 'ipsum',
};