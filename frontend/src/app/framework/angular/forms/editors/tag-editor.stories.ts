/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { LocalizerService, RootViewComponent, TagEditorComponent } from '@app/framework';

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
                (dropdownOpen)="load()">
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
        change: {
            action:'ngModelChange',
        },
    },
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <sqx-tag-editor
                    [allowOpen]="allowOpen"
                    [disabled]="disabled"
                    [itemsSource]="itemsSource"
                    [itemsSourceLoading]="itemsSourceLoading"
                    (ngModelChange)="change($event)"
                    [ngModel]="model"
                    [styleScrollable]="styleScrollable"
                    [styleBlank]="styleBlank"
                    [styleDashed]="styleDashed">
                </sqx-tag-editor>
            </sqx-root-view>
        `,
    }),
    decorators: [
        moduleMetadata({
            declarations: [
                TestComponent,
            ],
            imports: [
                FormsModule,
                RootViewComponent,
            ],
            providers: [
                {
                    provide: LocalizerService,
                    useFactory: () => new LocalizerService(TRANSLATIONS),
                },
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<TagEditorComponent & { model: any }>;

export const Default: Story = {
    args: {},
};

export const Suggestions: Story = {
    args: {
        itemsSource: ['A', 'B', 'C'],
        allowOpen: true,
    },
};

export const SuggestionsEmpty: Story = {
    args: {
        itemsSource: [],
        allowOpen: true,
    },
};

export const SuggestionsLoading: Story = {
    args: {
        itemsSourceLoading: true,
        allowOpen: true,
    },
};

export const Values: Story = {
    args: {
        itemsSource: [],
        model: ['A', 'A', 'B'],
    },
};

export const StyleDashed: Story = {
    args: {
        styleDashed: true,
        model: [],
    },
};

export const StyleDashedValues: Story = {
    args: {
        styleDashed: true,
        model: ['A', 'B', 'C'],
    },
};

export const StyleBlank: Story = {
    args: {
        styleBlank: true,
        model: [],
    },
};

export const StyleBlankValues: Story = {
    args: {
        styleBlank: true,
        model: ['A', 'B', 'C'],
    },
};

export const Multiline: Story = {
    args: {
        styleScrollable: false,
        model: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing', 'elit', 'sed', 'do', 'eiusmod', 'tempor', 'incididunt', 'ut', 'labore', 'et', 'dolore', 'magna', 'aliqua'],
    },
};

export const SingleLine: Story = {
    args: {
        styleScrollable: true,
        model: ['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing', 'elit', 'sed', 'do', 'eiusmod', 'tempor', 'incididunt', 'ut', 'labore', 'et', 'dolore', 'magna', 'aliqua'],
    },
};

export const Lazy: Story = {
    render: args => ({
        props: args,
        template: `
            <sqx-tag-editor-test></sqx-tag-editor-test>
        `,
    }),
};