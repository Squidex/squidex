/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { LanguageSelectorComponent, RootViewComponent } from '@app/framework';

export default {
    title: 'Framework/Language-Selector',
    component: LanguageSelectorComponent,
    argTypes: {
        size: {
            control: 'select',
            options: [
                'sm',
                'md',
                'lg',
            ],
        },
    },
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div class="text-center">
                    <sqx-language-selector
                        [exists]="exists"
                        [language]="language"    
                        [languages]="languages"
                        [percents]="percents">
                    </sqx-language-selector>
                </div>
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

type Story = StoryObj<LanguageSelectorComponent>;

export const Empty: Story = {
    args: {
        languages: [],
    },
};

export const OneLanguage: Story = {
    args: {
        languages: [
            { iso2Code: 'en', englishName: 'English' },
        ],
    },
};

export const FewLanguages: Story = {
    args: {
        languages: [
            { iso2Code: 'en', englishName: 'English' },
            { iso2Code: 'it', englishName: 'Italian' },
            { iso2Code: 'es', englishName: 'Spanish' },
        ],
    },
};

export const FewLanguagesWithExists: Story = {
    args: {
        languages: [
            { iso2Code: 'en', englishName: 'English' },
            { iso2Code: 'it', englishName: 'Italian' },
            { iso2Code: 'es', englishName: 'Spanish' },
        ],
        exists: {
            en: true,
            it: false,
            es: true,
        },
    },
};

export const ManyLanguages: Story = {
    args: {
        languages: [
            { iso2Code: 'en', englishName: 'English' },
            { iso2Code: 'it', englishName: 'Italian' },
            { iso2Code: 'es', englishName: 'Spanish' },
            { iso2Code: 'de', englishName: 'German' },
            { iso2Code: 'ru', englishName: 'Russian' },
        ],
    },
};

export const ManyLanguagesWithExists: Story = {
    args: {
        languages: [
            { iso2Code: 'en', englishName: 'English' },
            { iso2Code: 'it', englishName: 'Italian' },
            { iso2Code: 'es', englishName: 'Spanish' },
            { iso2Code: 'de', englishName: 'German' },
            { iso2Code: 'ru', englishName: 'Russian' },
        ],
        exists: {
            en: true,
            it: false,
            es: true,
            de: false,
            ru: true,
        },
    },
};

export const WithPercents: Story = {
    args: {
        languages: [
            { iso2Code: 'en', englishName: 'English' },
            { iso2Code: 'it', englishName: 'Italian' },
            { iso2Code: 'es', englishName: 'Spanish' },
        ],
        percents: {
            'en': 100,
            'it': 67,
        },
    },
};