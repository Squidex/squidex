/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { map, Observable, timer } from 'rxjs';
import { AutocompleteComponent, AutocompleteSource, LocalizerService, RootViewComponent } from '@app/framework';

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
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <sqx-autocomplete 
                    [disabled]="disabled"
                    [icon]="icon"
                    [inputStyle]="inputStyle"
                    [itemsSource]="itemsSource">
                </sqx-autocomplete>
            </sqx-root-view>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
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

type Story = StoryObj<AutocompleteComponent>;

export const Default: Story = {
    args: {
        itemsSource: new Source(['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing']),
    },
};

export const Disabled: Story = {
    args: {
        disabled: true,
    },
};

export const Icon: Story = {
    args: {
        icon: 'user',
    },
};

export const StyleEmpty: Story = {
    args: {
        inputStyle: 'empty',

    },
};

export const StyleUnderlined: Story = {
    args: {
        inputStyle: 'underlined',
    },
};

export const IconLoading: Story = {
    args: {
        itemsSource: new Source(['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'], 4000),
        icon: 'user',
    },
};