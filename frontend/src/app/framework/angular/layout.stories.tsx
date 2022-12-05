/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { moduleMetadata } from '@storybook/angular';
import { Meta, Story } from '@storybook/angular/types-6-0';
import { LayoutComponent, LocalizerService, SqxFrameworkModule } from '@app/framework';

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
    decorators: [
        moduleMetadata({
            imports: [
                BrowserAnimationsModule,
                SqxFrameworkModule,
                SqxFrameworkModule.forRoot(),
            ],
            providers: [
                { provide: LocalizerService, useValue: new LocalizerService({}) },
            ],
        }),
    ],
} as Meta;

const Template: Story<LayoutComponent> = (args: LayoutComponent) => ({
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
});

const ComplexTemplate: Story<LayoutComponent> = (args: LayoutComponent) => ({
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
});

export const Empty = Template.bind({});

Empty.args = {
    titleText: 'Title',
};

export const Icon = Template.bind({});

Icon.args = {
    titleText: 'Title',
    titleIcon: 'help',
};

export const InnerWidth = Template.bind({});

InnerWidth.args = {
    titleText: 'Title',
    titleIcon: '',
    innerWidth: 30,
    layout: 'main',
};

export const Left = Template.bind({});

Left.args = {
    titleText: 'Title',
    titleCollapsed: 'I am collapsed',
    layout: 'left',
};

export const Right = Template.bind({});

Right.args = {
    titleText: 'Title',
    titleCollapsed: 'I am collapsed',
    layout: 'right',
};

export const Complex = ComplexTemplate.bind({});

Complex.args = {
    titleText: 'Main',
    titleIcon: 'help',
    innerWidth: 20,
};