/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { LocalizerService, ResizeService } from '@app/framework/internal';
import { MenuItemComponent } from './menu-item.component';
import { MenuComponent } from './menu.component';
import { RootViewComponent } from './modals/root-view.component';

export default {
    title: 'Framework/Menu',
    component: MenuComponent,
    args: {
        small: false,
    },
    argTypes: {
        alignment: {
            control: 'radio',
            options: [
                'left',
                'right',
            ],
        },
        small: {
            control: 'boolean',
        },
    },
    decorators: [
        moduleMetadata({
            imports: [
                MenuItemComponent,
                RootViewComponent,
            ],
            providers: [
                {
                    provide: LocalizerService,
                    useFactory: () => new LocalizerService({}),
                },
                ResizeService,
            ],
        }),
    ],
} as Meta;

type Story = StoryObj<MenuComponent>;

export const Simple: Story = {
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div style="width: 400px; padding: 10px; border: 1px solid #ddd;">
                    <sqx-menu [alignment]="alignment" [items]="items" [small]="small">
                        <sqx-menu-item label="Menu Item1" />
                        <sqx-menu-item label="Menu Item2" icon="close" />
                        <sqx-menu-item label="Menu Item3" disabled />
                    </sqx-menu>
                </div>
            </sqx-root-view>
        `,
    }),
};

export const Overlap: Story = {
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div style="width: 400px; padding: 10px; border: 1px solid #ddd;">
                    <sqx-menu [alignment]="alignment" [items]="items" [small]="small">
                        <sqx-menu-item label="Menu Item1" />
                        <sqx-menu-item label="Menu Item2" icon="close" />
                        <sqx-menu-item label="Menu Item3" disabled />
                        <sqx-menu-item label="Menu Item4" />
                        <sqx-menu-item label="Menu Item5" />
                    </sqx-menu>
                </div>
            </sqx-root-view>
        `,
    }),
};

export const Custom: Story = {
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div style="width: 500px; padding: 10px; border: 1px solid #ddd;">
                    <sqx-menu [alignment]="alignment" [items]="items" [small]="small">
                        <sqx-menu-item label="Menu Item1" />

                        <button class="btn btn-primary">Custom 1</button>
                        <button class="btn btn-primary">Custom 2</button>

                        <sqx-menu-item label="Menu Item2" />
                    </sqx-menu>
                </div>
            </sqx-root-view>
        `,
    }),
};

export const CustomOverlap: Story = {
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div style="width: 400px; padding: 10px; border: 1px solid #ddd;">
                    <sqx-menu [alignment]="alignment" [items]="items" [small]="small">
                        <sqx-menu-item label="Menu Item1" />
                        <sqx-menu-item label="Menu Item2" icon="close" />
                        <sqx-menu-item label="Menu Item3" disabled />

                        <button class="btn btn-primary">Custom 1</button>
                        <button class="btn btn-primary">Custom 2</button>

                        <sqx-menu-item label="Menu Item4" />
                        <sqx-menu-item label="Menu Item5" />
                    </sqx-menu>
                </div>
            </sqx-root-view>
        `,
    }),
};