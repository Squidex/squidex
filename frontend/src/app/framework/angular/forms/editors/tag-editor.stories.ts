
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
                [itemsSource]="itemsSource"
                [itemsSourceLoading]="itemsSourceLoading"
                (open)="load()">
            </sqx-tag-editor>
        </sqx-root-view>
    `,
})
class TestComponent {
    public itemsSource: string[] = [];
    public itemsSourceLoading = false;

    public load() {
        this.itemsSource = [];
        this.itemsSourceLoading = true;

        setTimeout(() => {
            this.itemsSource = ['A', 'B'];
            this.itemsSourceLoading = false;
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
                [itemsSource]="itemsSource"
                [itemsSourceLoading]="itemsSourceLoading"
                [ngModel]="ngModel"
                [styleScrollable]="styleScrollable"
                [styleBlank]="styleBlank"
                [styleDashed]="styleDashed">
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
    itemsSource: ['A', 'B', 'C'],
    allowOpen: true,
};

export const SuggestionsEmpty = Template.bind({});

SuggestionsEmpty.args = {
    itemsSource: [],
    allowOpen: true,
};

export const SuggestionsLoading = Template.bind({});

SuggestionsLoading.args = {
    itemsSourceLoading: true,
    allowOpen: true,
};

export const Values = Template.bind({});

Values.args = {
    itemsSource: [],
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
    styleScrollable: false,
    ngModel: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing', 'elit', 'sed', 'do', 'eiusmod', 'tempor', 'incididunt', 'ut', 'labore', 'et', 'dolore', 'magna', 'aliqua'],
};

export const SingleLine = Template.bind({});

SingleLine.args = {
    styleScrollable: true,
    ngModel: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing', 'elit', 'sed', 'do', 'eiusmod', 'tempor', 'incididunt', 'ut', 'labore', 'et', 'dolore', 'magna', 'aliqua'],
};

export const Lazy = Template2.bind({});