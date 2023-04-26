/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { CodeEditorComponent, ScriptCompletions, SqxFrameworkModule } from '@app/framework';

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

const Template: Story<CodeEditorComponent & { model: any }> = (args: CodeEditorComponent) => ({
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
});

const SingleLineTemplate: Story<CodeEditorComponent & { model: any }> = (args: CodeEditorComponent) => ({
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
});

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

export const Default = Template.bind({});

Default.args = {
    height: 'auto',
};

export const SingleLine = SingleLineTemplate.bind({});

SingleLine.args = {
    completion: COMPLETIONS,
    singleLine: true,
};