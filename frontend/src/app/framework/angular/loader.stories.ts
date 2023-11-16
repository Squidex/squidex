/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { LoaderComponent } from '@app/framework';

export default {
    title: 'Framework/Loader',
    component: LoaderComponent,
    argTypes: {
        size: {
            control: 'number',
        },
        color: {
            control: 'select',
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
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<LoaderComponent>;

export const ColorWhite: Story = {
    args: {
        color: 'white',
    },
};

export const ColorTheme: Story = {
    args: {
        color: 'theme',
    },
};

export const ColorText: Story = {
    args: {
        color: 'text',
    },
};

export const ColorInput: Story = {
    args: {
        color: 'input',
    },
};

export const SizeSmall: Story = {
    args: {
        size: 16,
    },
};

export const SizeMedium: Story = {
    args: {
        size: 32,
    },
};

export const SizeLarge: Story = {
    args: {
        size: 64,
    },
};