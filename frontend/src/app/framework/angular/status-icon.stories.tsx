/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, Story } from '@storybook/angular/types-6-0';
import { StatusIconComponent } from '@app/framework';

export default {
    title: 'Framework/StatusIcon',
    component: StatusIconComponent,
    argTypes: {
        status: {
            control: 'enum',
            options: [
                'Failed',
                'Success',
                'Completed',
                'Pending',
            ],
        },
        statusText: {
            control: 'text',
        },
        size: {
            control: 'enum',
            options: [
                'sm',
                'md',
                'lg',
            ],
        },
    },
} as Meta;

const Template: Story<StatusIconComponent> = (args: StatusIconComponent) => ({
    props: args,
});

export const Success = Template.bind({});

Success.args = {
    status: 'Success',
};

export const Completed = Template.bind({});

Completed.args = {
    status: 'Completed',
};

export const Failed = Template.bind({});

Failed.args = {
    status: 'Failed',
};

export const Pending = Template.bind({});

Pending.args = {
    status: 'Pending',
};

export const Started = Template.bind({});

Started.args = {
    status: 'Started',
};

export const SizeSm = Template.bind({});

SizeSm.args = {
    size: 'sm',
};

export const SizeLg = Template.bind({});

SizeLg.args = {
    size: 'lg',
};