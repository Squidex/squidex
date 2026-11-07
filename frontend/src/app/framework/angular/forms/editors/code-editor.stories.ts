/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { CodeEditorComponent, ScriptCompletions } from '@app/framework';

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
        singleLine: {
            control: false,
        },
        change: {
            action: 'valueChange',
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
                    [mode]="mode"
                    (valueChange)="change($event)"
                    [value]="model"
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

type Story = StoryObj<CodeEditorComponent & { model: any }>;

export const Default: Story = {
    args: {
        height: 'auto',
        model: 'Value',
    } as any,
};

export const Completions: Story = {
    args: {
        height: 200,
        completion: COMPLETIONS,
    },
};

export const InvalidJson: Story = {
    args: {
        height: 200,
        model: 'Invalid JSON\n{ \"Hello\": 42 }',
        mode: 'ace/mode/javascript',
    } as any,
};

export const InvalidJsonWithoutAnnotations: Story = {
    args: {
        height: 200,
        model: 'Invalid JSON\n{ \"Hello\": 42 }',
        mode: 'ace/mode/javascript',
        disabled: true,
    } as any,
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
                        (valueChange)="change($event)"
                        [value]="model"
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
    } as any,
};