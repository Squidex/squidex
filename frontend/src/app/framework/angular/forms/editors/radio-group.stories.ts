/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, StoryObj } from '@storybook/angular';
import { RadioGroupComponent } from '@app/framework';

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
        change: {
            action: 'valueChange',
        },
    },
    render: args => ({
        props: args,
        template: `
            <div style="padding: 2rem; max-width: 400px">
                <sqx-radio-group
                    [disabled]="disabled"
                    [layout]="layout"
                    (valueChange)="change($event)"
                    [value]="model"
                    [unsorted]="unsorted"
                    [values]="values">
                </sqx-radio-group>
            </div>
        `,
    }),
} as Meta;

type Story = StoryObj<RadioGroupComponent & { model: any }>;

export const Default: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
        model: [],
    },
};

export const Unsorted: Story = {
    args: {
        values: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'],
        unsorted: false,
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
        model: 'ipsum',
    },
};