/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { AvatarComponent } from '@app/framework';

export default {
    title: 'Framework/Avatar',
    component: AvatarComponent,
    argTypes: {
        image: {
            control: 'text',
        },
        identifier: {
            control: 'text',
        },
        size: {
            control: 'number',
        },
    },
    decorators: [
        moduleMetadata({
            imports: [
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<AvatarComponent>;

export const Empty: Story = {
    args: {},
};

export const Image = {
    args: {
        image: 'https://via.placeholder.com/50',
    },
};

export const InvalidImage = {
    args: {
        image: 'https://invalid-url',
    },
};

export const Large = {
    args: {
        identifier: 'image',
        size: 200,
    },
};

export const InvalidImageWithFallback = {
    args: {
        image: 'https://invalid-url',
        identifier: 'image',
    },
};