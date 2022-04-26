/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { LocalizerService, SqxFrameworkModule, TagEditorComponent } from '@app/framework';

const TRANSLATIONS = {
    'common.tagAdd': ', to add tag',
};

export default {
    title: 'Framework/TagEditor',
    component: TagEditorComponent,
    argTypes: {
        dashed: {
            control: 'boolean',
        },
        disabled: {
            control: 'boolean',
        },
    },
    decorators: [
        moduleMetadata({
            imports: [
                SqxFrameworkModule,
                SqxFrameworkModule.forRoot(),
            ],
            providers: [
                { provide: LocalizerService, useFactory: () => new LocalizerService(TRANSLATIONS) },
            ],
        }),
    ],
} as Meta;

const Template: Story<TagEditorComponent & { ngModel: any }> = (args: TagEditorComponent) => ({
    props: args,
    template: `
        <sqx-root-view>
            <sqx-tag-editor 
                [dashed]="dashed"
                [disabled]="disabled"
                [ngModel]="ngModel"
                [singleLine]="singleLine"
                [styleBlank]="styleBlank"
                [suggestions]="suggestions">
            </sqx-tag-editor>
        </sqx-root-view>
    `,
});

export const Default = Template.bind({});

export const Suggestions = Template.bind({});

Suggestions.args = {
    suggestions: ['A', 'B', 'C'],
};

export const Values = Template.bind({});

Values.args = {
    ngModel: ['A', 'A', 'B'],
};

export const Dashed = Template.bind({});

Dashed.args = {
    dashed: true,
};

export const Blank = Template.bind({});

Blank.args = {
    styleBlank: true,
};

export const Multiline = Template.bind({});

Multiline.args = {
    singleLine: false,
    ngModel: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing', 'elit', 'sed', 'do', 'eiusmod', 'tempor', 'incididunt', 'ut', 'labore', 'et', 'dolore', 'magna', 'aliqua'],
};

export const SingleLine = Template.bind({});

SingleLine.args = {
    singleLine: true,
    ngModel: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing', 'elit', 'sed', 'do', 'eiusmod', 'tempor', 'incididunt', 'ut', 'labore', 'et', 'dolore', 'magna', 'aliqua'],
};