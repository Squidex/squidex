/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { LoaderComponent, SqxFrameworkModule } from '@app/framework';

export default {
    title: 'Framework/Loader',
    component: LoaderComponent,
    argTypes: {
        size: {
            control: 'number',
        },
        color: {
            control: 'enum',
            options: [
                'white',
                'theme',
                'text',
            ],
        },
    },
    decorators: [
        moduleMetadata({
            imports: [
                SqxFrameworkModule,
                SqxFrameworkModule.forRoot(),
            ],
        }),
    ],
} as Meta;

const Template: Story<LoaderComponent> = (args: LoaderComponent) => ({
    props: args,
});

export const ColorWhite = Template.bind({});

ColorWhite.args = {
    color: 'white',
};

export const ColorTheme = Template.bind({});

ColorTheme.args = {
    color: 'theme',
};

export const ColorText = Template.bind({});

ColorText.args = {
    color: 'text',
};

export const ColorInput = Template.bind({});

ColorInput.args = {
    color: 'input',
};

export const SizeSmall = Template.bind({});

SizeSmall.args = {
    size: 16,
};

export const SizeMedium = Template.bind({});

SizeMedium.args = {
    size: 32,
};

export const SizeLarge = Template.bind({});

SizeLarge.args = {
    size: 64,
};