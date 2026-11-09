/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { CodeEditorComponent, ColorPickerComponent, RootViewComponent } from '@app/framework';

export default {
    title: 'Framework/ColorPicker',
    component: ColorPickerComponent,
    argTypes: {
        disabled: {
            control: 'boolean',
        },
        change: {
            action: 'valueChange',
        },
    },
    args: {
        mode: 'Input',
    },
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <sqx-color-picker 
                    [disabled]="disabled"
                    [mode]="mode"
                    (valueChange)="change($event)"
                    [value]="value">
                </sqx-color-picker>
            </sqx-root-view>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                RootViewComponent,
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<CodeEditorComponent>;

export const Default: Story = {
    args: {
        value: '#ff0000',
    } as any,
};

export const Name: Story = {
    args: {
        value: 'red',
    } as any,
};

export const Circle: Story = {
    args: {
        value: 'red',
        mode: 'Circle',
    } as any,
};