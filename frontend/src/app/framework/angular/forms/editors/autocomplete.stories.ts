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
                <div style="margin-top: 100px">
                    <sqx-autocomplete 
                        [debounceTime]="debounceTime"
                        [disabled]="disabled"
                        [dropdownFullWidth]="dropdownFullWidth"
                        [icon]="icon"
                        [inputStyle]="inputStyle"
                        [itemsSource]="itemsSource"
                        [startCharacter]="startCharacter"
                        [textArea]="textArea">
                    </sqx-autocomplete>
                </div>
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
        return timer(this.delay).pipe(map(() => this.values.filter(x => query.length > 0 && x.indexOf(query) >= 0)));
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

export const StyleFullWidth: Story = {
    args: {
        itemsSource: new Source(['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing']),
        dropdownFullWidth: true,
    },
};

export const IconLoading: Story = {
    args: {
        itemsSource: new Source(['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing'], 2000),
        icon: 'user',
    },
};

export const StartCharacter: Story = {
    args: {
        debounceTime: 0,
        itemsSource: new Source(['donald@duck.com', 'scrooge@mcduck.com']),
        startCharacter: '@',
    },
};

export const TextArea: Story = {
    args: {
        debounceTime: 0,
        itemsSource: new Source(['Lorem', 'ipsum', 'dolor', 'sit', 'amet', 'consectetur', 'adipiscing']),
        startCharacter: '@',
        textArea: true,
    },
};