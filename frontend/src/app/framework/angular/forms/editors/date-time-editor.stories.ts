/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { LocalizerService, SqxFrameworkModule, UIOptions } from '@app/framework';
import { DateTimeEditorComponent } from './date-time-editor.component';

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
                SqxFrameworkModule,
                SqxFrameworkModule.forRoot(),
            ],
            providers: [
                { provide: LocalizerService, useFactory: () => new LocalizerService(translations) },
                { provide: UIOptions, useFactory: () => new UIOptions({}) },
            ],
        }),
    ],
} as Meta;

const Template: Story<DateTimeEditorComponent> = (args: DateTimeEditorComponent) => ({
    props: args,
});

export const Date = Template.bind({});

Date.args = {
    mode: 'Date',
};

export const DateTime = Template.bind({});

DateTime.args = {
    mode: 'DateTime',
};