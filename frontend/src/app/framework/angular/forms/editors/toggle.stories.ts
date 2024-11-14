/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormsModule } from '@angular/forms';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { RadioGroupComponent, ToggleComponent } from '@app/framework';

export default {
    title: 'Framework/Toggle',
    component: ToggleComponent,
    argTypes: {
        disabled: {
            control: 'boolean',
        },
        change: {
            action:'ngModelChange',
        },
    },
    render: args => ({
        props: args,
        template: `
            <sqx-toggle
                [disabled]="disabled"
                (ngModelChange)="change($event)"
                [ngModel]="model">
            </sqx-toggle>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                FormsModule,
            ],
        }),
    ],
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