/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { DateTimeEditorComponent, LocalizerService, UIOptions } from '@app/framework';

const translations = {
    'common.date': 'Date',
    'common.dateTimeEditor.local': 'Local',
    'common.dateTimeEditor.now': 'Now',
    'common.dateTimeEditor.today': 'Today',
    'common.dateTimeEditor.utc': 'UTC',
    'common.time': 'Time',
};

export default {
    title: 'Framework/DateTimeEditor',
    component: DateTimeEditorComponent,
    argTypes: {
        disabled: {
            control: 'boolean',
        },
        hideClear: {
            control: 'boolean',
        },
        hideDateButtons: {
            control: 'boolean',
        },
        hideDateTimeModeButton: {
            control: 'boolean',
        },
        mode: {
            control: 'radio',
            options: [
                'Date',
                'DateTime',
            ],
        },
    },
    decorators: [
        moduleMetadata({
            imports: [
            ],
            providers: [
                {
                    provide: LocalizerService,
                    useFactory: () => new LocalizerService(translations),
                },
                {
                    provide: UIOptions,
                    useFactory: () => new UIOptions({}),
                },
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<DateTimeEditorComponent>;

export const Date: Story = {
    args: {
        mode: 'Date',
    },
};

export const DateTime: Story = {
    args: {
        mode: 'DateTime',
        disabled: true,
    },
};

export const DateTimeDisabled: Story = {
    args: {
        mode: 'DateTime',
        disabled: true,
    },
};