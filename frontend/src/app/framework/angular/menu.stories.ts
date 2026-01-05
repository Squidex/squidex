/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { action } from '@storybook/addon-actions';
import { Meta, moduleMetadata, StoryObj } from '@storybook/angular';
import { LocalizerService, ResizeService } from '@app/framework/internal';
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
    render: args => ({
        props: args,
        template: `
            <sqx-root-view>
                <div style="width: 400px; padding: 10px; border: 1px solid #ddd;">
                    <sqx-menu [alignment]="alignment" [items]="items" [small]="small" />
                </div>
            </sqx-root-view>
        `,
    }),
    decorators: [
        moduleMetadata({
            imports: [
                BrowserAnimationsModule,
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
    args: {
        items: [{
            key: 'item1',
            label: 'Menu Item1',
            onClick: action('Item1'),
        }, {
            key: 'item2',
            icon: 'close',
            label: 'Menu Item2',
            onClick: action('Item2'),
        }, {
            key: 'item3',
            icon: 'close',
            onClick: action('Item3'),
            isDisabled: true,
        }],
    },
};

export const Overlap: Story = {
    args: {
        items: [{
            key: 'item1',
            label: 'Menu Item1',
            onClick: action('Item1'),
        }, {
            key: 'item2',
            label: 'Menu Item2',
            onClick: action('Item2'),
            showAlways: true,
        }, {
            key: 'item3',
            label: 'Menu Item3',
            onClick: action('Item3'),
        }, {
            key: 'item4',
            label: 'Menu Item4',
            onClick: action('Item4'),
        }, {
            key: 'item5',
            label: 'Menu Item5',
            onClick: action('Item5'),
        }],
    },
};