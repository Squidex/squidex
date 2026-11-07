/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, StoryObj } from '@storybook/angular';
import { RadioGroupComponent, ToggleComponent } from '@app/framework';

export default {
    title: 'Framework/Toggle',
    component: ToggleComponent,
    argTypes: {
        disabled: {
            control: 'boolean',
        },
        change: {
            action: 'valueChange',
        },
    },
    render: args => ({
        props: args,
        template: `
            <sqx-toggle
                [disabled]="disabled"
                (valueChange)="change($event)"
                [value]="model">
            </sqx-toggle>
        `,
    }),
} as Meta;

type Story = StoryObj<RadioGroupComponent & { model: any }>;

export const Default: Story = {};

export const Checked: Story = {
    args: {
        model: true,
    },
};

export const Unchecked: Story = {
    args: {
        model: false,
    },
};