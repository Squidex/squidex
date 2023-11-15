/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { CodeEditorComponent, ScriptCompletions, SqxFrameworkModule } from '@app/framework/internal';

export default {
    title: 'Framework/CodeEditor',
    component: CodeEditorComponent,
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
            <div>
                <sqx-code-editor 
                    [borderless]="borderless"
                    [disabled]="disabled"
                    [height]="height"
                    [maxLines]="maxLines"
                    [singleLine]="singleLine"
                    [valueFile]="valueFile"
                    [valueMode]="valueMode"
                    [wordWrap]="wordWrap">
                </sqx-code-editor>
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

const COMPLETIONS: ScriptCompletions = [{
    path: 'path1',
    description: 'Test1 Path',
    type: 'Any',
}, {
    path: 'path2',
    description: 'Test2 Path',
    type: 'Array',
}, {
    path: 'path3',
    description: 'Test3 Path',
    type: 'String',
}];

type Story = StoryObj<CodeEditorComponent>;

export const Default: Story = {
    args: {
        height: 'auto',
    },
};

export const Completions: Story = {
    args: {
        completion: COMPLETIONS,
    },
};

export const SingleLine: Story = {
    render: args => ({
        props: args,
        template: `
            <div class="row">
                <div class="col">
                    <sqx-code-editor 
                        [borderless]="borderless"
                        [completion]="completion"
                        [disabled]="disabled"
                        [height]="height"
                        [maxLines]="maxLines"
                        [singleLine]="singleLine"
                        [valueFile]="valueFile"
                        [valueMode]="valueMode"
                        [wordWrap]="wordWrap">
                    </sqx-code-editor>
                </div>
                <div class="col">
                    <input class="form-control">
                </div>
            </div>
        `,
    }),
    args: {
        singleLine: true,
    },
};