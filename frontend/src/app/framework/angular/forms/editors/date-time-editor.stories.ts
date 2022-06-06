/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { DateTimeEditorComponent, LocalizerService, SqxFrameworkModule, UIOptions } from '@app/framework';

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
                BrowserAnimationsModule,
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

export const DateDisabled = Template.bind({});

DateDisabled.args = {
    mode: 'Date',
    disabled: true,
};

export const DateTime = Template.bind({});

DateTime.args = {
    mode: 'DateTime',
};

export const DateTimeDisabled = Template.bind({});

DateTimeDisabled.args = {
    mode: 'DateTime',
    disabled: true,
};