/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    HelpComponent,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    WebhookComponent,
    WebhookEventsPageComponent,
    WebhooksPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: WebhooksPageComponent,
        children: [
            {
                path: 'events',
                component: WebhookEventsPageComponent
            },
            {
                path: 'help',
                component: HelpComponent,
                data: {
                    helpPage: '05-integrated/webhooks'
                }
            }
        ]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        WebhookComponent,
        WebhookEventsPageComponent,
        WebhooksPageComponent
    ]
})
export class SqxFeatureWebhooksModule { }