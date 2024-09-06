/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { EditableTitleComponent, LocalizerService } from '@app/framework';

export default {
    title: 'Framework/EditableTitle',
    component: EditableTitleComponent,
    argTypes: {
        inputTitle: {
            control: 'text',
        },
        closeButton: {
            control: 'boolean',
        },
        inputTitleRequired: {
            control: 'boolean',
        },
        inputTitleLength: {
            control: 'number',
        },
        size: {
            control: 'select',
            options: [
                'sm',
                'md',
                'lg',
            ],
        },
    },
    args: {
        closeButton: true,
        inputTitleLength: 30,
        inputTitleRequired: true,
    },
    render: args => ({
        props: args,
        template: `
            <div class="card mt-4">
                <div class="card-body">
                    <div class="row" style="flex-wrap: nowrap">
                        <div class="col-9">
                            <sqx-editable-title 
                                [closeButton]="closeButton"
                                [size]="size"
                                [inputTitle]="inputTitle"
                                [inputTitleLength]="inputTitleLength"
                                [inputTitleRequired]="inputTitleRequired">
                            </sqx-editable-title>
                        </div>
                        <div class="col-3">
                            <button class="btn btn-primary btn-{{size}}">
                                Button
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                FormsModule,
                ReactiveFormsModule,
            ],
            providers: [
                {
                    provide: LocalizerService,
                    useValue: new LocalizerService({}),
                },
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<EditableTitleComponent>;

export const Default: Story = {
    args: {
        inputTitle: 'My Title',
        size: 'md',
    },
};

export const DefaultNoCloseButton: Story = {
    args: {
        inputTitle: 'My Title',
        size: 'md',
        closeButton: false,
    },
};

export const Small: Story = {
    args: {
        inputTitle: 'My Title',
        size: 'sm',
    },
};

export const SmallNoCloseButton: Story = {
    args: {
        inputTitle: 'My Title',
        size: 'sm',
        closeButton: false,
    },
};

export const Large: Story = {
    args: {
        inputTitle: 'My Title',
        size: 'lg',
    },
};

export const LargeNoCloseButton: Story = {
    args: {
        inputTitle: 'My Title',
        size: 'lg',
        closeButton: false,
    },
};

export const LongTitle: Story = {
    args: {
        inputTitle: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua',
        size: 'md',
    },
};