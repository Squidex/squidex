/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, Story } from '@storybook/angular/types-6-0';
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
} as Meta;

const Template: Story<AvatarComponent> = (args: AvatarComponent) => ({
    props: args,
});

export const Empty = Template.bind({});

Empty.args = {
};

export const Image = Template.bind({});

Image.args = {
    image: 'https://via.placeholder.com/50',
};

export const InvalidImage = Template.bind({});

InvalidImage.args = {
    image: 'https://invalid-url',
};

export const Identifier = Template.bind({});

Identifier.args = {
    identifier: 'image',
};

export const Large = Template.bind({});

Large.args = {
    identifier: 'image',
    size: 200,
};

export const InvalidImageWithFallback = Template.bind({});

InvalidImageWithFallback.args = {
    image: 'https://invalid-url',
    identifier: 'image',
};