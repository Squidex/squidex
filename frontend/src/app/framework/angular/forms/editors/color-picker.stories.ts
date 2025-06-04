/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormsModule } from '@angular/forms';
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
            action: 'ngModelChange',
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
                    (ngModelChange)="change($event)"
                    [ngModel]="ngModel">
                </sqx-color-picker>
            </sqx-root-view>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                FormsModule,
                RootViewComponent,
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<CodeEditorComponent>;

export const Default: Story = {
    args: {
        ngModel: '#ff0000',
    } as any,
};

export const Name: Story = {
    args: {
        ngModel: 'red',
    } as any,
};

export const Circle: Story = {
    args: {
        ngModel: 'red',
        mode: 'Circle',
    } as any,
};