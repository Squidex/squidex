/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { EditableTitleComponent, LocalizerService, SqxFrameworkModule } from '@app/framework';

export default {
    title: 'Framework/EditableTitle',
    component: EditableTitleComponent,
    argTypes: {
        inputTitle: {
            control: 'inputTitle',
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
    decorators: [
        moduleMetadata({
            imports: [
                BrowserAnimationsModule,
                FormsModule,
                ReactiveFormsModule,
                SqxFrameworkModule,
                SqxFrameworkModule.forRoot(),
            ],
            providers: [
                { provide: LocalizerService, useValue: new LocalizerService({}) },
            ],
        }),
    ],
} as Meta;

const Template: Story<EditableTitleComponent> = (args: EditableTitleComponent) => ({
    props: args,
    template: `
        <div class="card mt-4">
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
    `,
});

export const Default = Template.bind({});

Default.args = {
    inputTitle: 'My Title',
    size: 'md',
};

export const DefaultNoCloseButton = Template.bind({});

DefaultNoCloseButton.args = {
    inputTitle: 'My Title',
    size: 'md',
    closeButton: false,
};

export const Small = Template.bind({});

Small.args = {
    inputTitle: 'My Title',
    size: 'sm',
};

export const SmallNoCloseButton = Template.bind({});

SmallNoCloseButton.args = {
    inputTitle: 'My Title',
    size: 'sm',
    closeButton: false,
};

export const Large = Template.bind({});

Large.args = {
    inputTitle: 'My Title',
    size: 'lg',
};

export const LargeNoCloseButton = Template.bind({});

LargeNoCloseButton.args = {
    inputTitle: 'My Title',
    size: 'lg',
    closeButton: false,
};

export const LongTitle = Template.bind({});

LongTitle.args = {
    inputTitle: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua',
    size: 'md',
};