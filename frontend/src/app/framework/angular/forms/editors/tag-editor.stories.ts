
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { LocalizerService, SqxFrameworkModule, TagEditorComponent } from '@app/framework';

const TRANSLATIONS = {
    'common.tagAdd': ', to add tag',
    'common.empty': 'Nothing available.',
};

@Component({
    selector: 'sqx-tag-editor-test',
    template: `
        <sqx-root-view>
            <sqx-tag-editor 
                [allowOpen]="true" 
                [suggestions]="suggestions"
                [suggestionsLoading]="suggestionsLoading"
                (open)="load()">
            </sqx-tag-editor>
        </sqx-root-view>
    `,
})
class TestComponent {
    public suggestions: string[] = [];
    public suggestionsLoading = false;

    public load() {
        this.suggestions = [];
        this.suggestionsLoading = true;

        setTimeout(() => {
            this.suggestions = ['A', 'B'];
            this.suggestionsLoading = false;
        }, 1000);
    }
}

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
            declarations: [
                TestComponent,
            ],
            imports: [
                BrowserAnimationsModule,
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
                [allowOpen]="allowOpen"
                [disabled]="disabled"
                [ngModel]="ngModel"
                [singleLine]="singleLine"
                [styleBlank]="styleBlank"
                [styleDashed]="styleDashed"
                [suggestions]="suggestions"
                [suggestionsLoading]="suggestionsLoading">
            </sqx-tag-editor>
        </sqx-root-view>
    `,
});

const Template2: Story<TagEditorComponent & { ngModel: any }> = (args: TagEditorComponent) => ({
    props: args,
    template: `
        <sqx-tag-editor-test></sqx-tag-editor-test>
    `,
});

export const Default = Template.bind({});

export const Suggestions = Template.bind({});

Suggestions.args = {
    suggestions: ['A', 'B', 'C'],
    allowOpen: true,
};

export const SuggestionsEmpty = Template.bind({});

SuggestionsEmpty.args = {
    suggestions: [],
    allowOpen: true,
};

export const SuggestionsLoading = Template.bind({});

SuggestionsLoading.args = {
    suggestionsLoading: true,
    allowOpen: true,
};

export const Values = Template.bind({});

Values.args = {
    suggestions: [],
    ngModel: ['A', 'A', 'B'],
};

export const StyleDashed = Template.bind({});

StyleDashed.args = {
    styleDashed: true,
    ngModel: [],
};

export const StyleDashedValues = Template.bind({});

StyleDashedValues.args = {
    styleDashed: true,
    ngModel: ['A', 'B', 'C'],
};

export const StyleBlank = Template.bind({});

StyleBlank.args = {
    styleBlank: true,
    ngModel: [],
};

export const StyleBlankValues = Template.bind({});

StyleBlankValues.args = {
    styleBlank: true,
    ngModel: ['A', 'B', 'C'],
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

export const Lazy = Template2.bind({});