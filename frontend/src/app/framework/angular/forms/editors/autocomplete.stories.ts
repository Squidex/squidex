/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { map, Observable, timer } from 'rxjs';
import { AutocompleteComponent, LocalizerService, SqxFrameworkModule } from '@app/framework';
import { AutocompleteSource } from './autocomplete.component';

const TRANSLATIONS = {
    'common.search': 'Search',
    'common.empty': 'Nothing available.',
};

export default {
    title: 'Framework/Autocomplete',
    component: AutocompleteComponent,
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
            providers: [
                { provide: LocalizerService, useFactory: () => new LocalizerService(TRANSLATIONS) },
            ],
        }),
    ],
} as Meta;

const Template: Story<AutocompleteComponent & { model: any }> = (args: AutocompleteComponent) => ({
    props: args,
    template: `
        <sqx-root-view>
            <sqx-autocomplete 
                [disabled]="disabled"
                [icon]="icon"
                [inputStyle]="inputStyle"
                [source]="source">
            </sqx-autocomplete>
        </sqx-root-view>
    `,
});

class Source implements AutocompleteSource {
    constructor(
        private readonly values: string[],
        private readonly delay = 0,
    ) {
    }

    public find(query: string): Observable<readonly any[]> {
        return timer(this.delay).pipe(map(() => this.values.filter(x => x.indexOf(query) >= 0)));
    }
}

export const Default = Template.bind({});

Default.args = {
    source: new Source(['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing']),
};

export const Disabled = Template.bind({});

Disabled.args = {
    disabled: true,
};

export const Icon = Template.bind({});

Icon.args = {
    icon: 'user',
};

export const StyleEmpty = Template.bind({});

StyleEmpty.args = {
    inputStyle: 'empty',
};

export const StyleUnderlined = Template.bind({});

StyleUnderlined.args = {
    inputStyle: 'underlined',
};

export const IconLoading = Template.bind({});

IconLoading.args = {
    source: new Source(['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'], 4000),
    icon: 'user',
};