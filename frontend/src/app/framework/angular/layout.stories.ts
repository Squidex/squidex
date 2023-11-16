/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { LayoutComponent, LayoutContainerDirective, ListViewComponent, LocalizerService, RootViewComponent } from '@app/framework';

export default {
    title: 'Framework/Layout',
    component: LayoutComponent,
    argTypes: {
        titleText: {
            control: 'text',
        },
        titleCollapsed: {
            control: 'text',
        },
        titleIcon: {
            control: 'select',
            options: [
                '',
                'help',
                'help2',
            ],
        },
        layout: {
            control: 'select',
            options: [
                'left',
                'main',
                'right',
            ],
            defaultValue: 'left',
        },
        innerWidth: {
            control: 'number',
        },
    },
    parameters: {
        layout: 'fullscreen',
    },
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div sqxLayoutContainer>
                    <sqx-layout
                        [layout]="layout"
                        [innerWidth]="innerWidth"
                        [titleCollapsed]="titleCollapsed"
                        [titleIcon]="titleIcon"
                        [titleText]="titleText">
                        <div>
                            <sqx-list-view [innerWidth]="innerWidth + 'rem'">
                                <div class="card">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                                <div class="card mt-2">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                                <div class="card mt-2">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                            </sqx-list-view>
                        </div>
                    </sqx-layout>
                </div>
            </sqx-root-view>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                LayoutContainerDirective,
                RootViewComponent,
                ListViewComponent,
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

type Story = StoryObj<LayoutComponent>;

export const Empty: Story = {
    args: {
        titleText: 'Title',
    },
};

export const Icon: Story = {
    args: {
        titleText: 'Title',
        titleIcon: 'help',
    },
};

export const InnerWidth: Story = {
    args: {
        titleText: 'Title',
        titleIcon: '',
        innerWidth: 30,
        layout: 'main',
    },
};

export const Left: Story = {
    args: {
        titleText: 'Title',
        titleCollapsed: 'I am collapsed',
        layout: 'left',
    },
};

export const Right: Story = {
    args: {
        titleText: 'Title',
        titleCollapsed: 'I am collapsed',
        layout: 'right',
    },
};

export const Complex: Story = {
    args: {
        titleText: 'Main',
        titleIcon: 'help',
        innerWidth: 20,
    },
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div sqxLayoutContainer>
                    <sqx-layout titleText="Left" layout="left" width="15">
                        <div>
                            <sqx-list-view>
                                <div class="card">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                                <div class="card mt-2">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                                <div class="card mt-2">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                            </sqx-list-view>
                        </div>
                    </sqx-layout>
                    <sqx-layout layout="main"
                        [innerWidth]="innerWidth"
                        [titleCollapsed]="titleCollapsed"
                        [titleIcon]="titleIcon"
                        [titleText]="titleText">
                        <div>
                            <sqx-list-view [innerWidth]="innerWidth + 'rem'">
                                <div class="card">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                                <div class="card mt-2">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                                <div class="card mt-2">
                                    <div class="card-body">
                                        Content
                                    </div>
                                </div>
                            </sqx-list-view>
                        </div>
                    </sqx-layout>
                    <sqx-layout titleText="Simple" layout="right" width="15">
                        <div class="p-4">
                            <div class="card">
                                <div class="card-body">
                                    Content
                                </div>
                            </div>
                            <div class="card mt-2">
                                <div class="card-body">
                                    Content
                                </div>
                            </div>
                            <div class="card mt-2">
                                <div class="card-body">
                                    Content
                                </div>
                            </div>
                        </div>
                    </sqx-layout>
                </div>
            </sqx-root-view>
        `,
    }),
};