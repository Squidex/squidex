/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormsModule } from '@angular/forms';
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
    args: {
        size: 'Normal',
    },
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
        change: {
            action: 'ngModelChange',
        },
        mode: {
            control: 'radio',
            options: [
                'Date',
                'DateTime',
            ],
        },
        size: {
            control: 'radio',
            options: [
                'Normal',
                'Compact',
                'Mini',
            ],
        },
    },
    render: args => ({
        props: args,
        template: `
            <sqx-date-time-editor 
                [disabled]="disabled"
                [hideClear]="hideClear"
                [hideDateButtons]="hideDateButtons"
                [hideDateTimeModeButton]="hideDateTimeModeButton"
                [mode]="mode"
                (ngModelChange)="change($event)"
                [ngModel]="ngModel"
                [size]="size">
            </sqx-date-time-editor>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                FormsModule,
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
export const DateValue: Story = {
    args: {
        mode: 'Date',
        ngModel: '2023-12-11',
    } as any,
};

export const DateTime: Story = {
    args: {
        mode: 'DateTime',
    },
};

export const DateTimeValue: Story = {
    args: {
        mode: 'DateTime',
        ngModel: '2023-12-11T10:09:08',
    } as any,
};

export const DateTimeValueUtc: Story = {
    args: {
        mode: 'DateTime',
        ngModel: '2023-12-11T10:09:08Z',
    } as any,
};

export const DateTimeDisabled: Story = {
    args: {
        mode: 'DateTime',
        disabled: true,
    },
};