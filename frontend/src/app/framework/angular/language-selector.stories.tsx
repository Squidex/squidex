/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { LanguageSelectorComponent, SqxFrameworkModule } from '@app/framework';

export default {
    title: 'Framework/Language-Selector',
    component: LanguageSelectorComponent,
    argTypes: {
        size: {
            control: 'enum',
            options: [
                'sm',
                'md',
                'lg',
            ],
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

const Template: Story<LanguageSelectorComponent> = (args: LanguageSelectorComponent) => ({
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
});

export const Empty = Template.bind({});

Empty.args = {
    languages: [],
};

export const OneLanguage = Template.bind({});

OneLanguage.args = {
    languages: [
        { iso2Code: 'en', englishName: 'English' },
    ],
};

export const FewLanguages = Template.bind({});

FewLanguages.args = {
    languages: [
        { iso2Code: 'en', englishName: 'English' },
        { iso2Code: 'it', englishName: 'Italian' },
        { iso2Code: 'es', englishName: 'Spanish' },
    ],
};

export const FewLanguagesWithExists = Template.bind({});

FewLanguagesWithExists.args = {
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
};

export const ManyLanguages = Template.bind({});

ManyLanguages.args = {
    languages: [
        { iso2Code: 'en', englishName: 'English' },
        { iso2Code: 'it', englishName: 'Italian' },
        { iso2Code: 'es', englishName: 'Spanish' },
        { iso2Code: 'de', englishName: 'German' },
        { iso2Code: 'ru', englishName: 'Russian' },
    ],
};

export const ManyLanguagesWithExists = Template.bind({});

ManyLanguagesWithExists.args = {
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
};

export const WithPercents = Template.bind({});

WithPercents.args = {
    languages: [
        { iso2Code: 'en', englishName: 'English' },
        { iso2Code: 'it', englishName: 'Italian' },
        { iso2Code: 'es', englishName: 'Spanish' },
    ],
    percents: {
        'en': 100,
        'it': 67,
    },
};