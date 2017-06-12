/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SqxFrameworkModule } from 'shared';

import {
    WebhooksPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: WebhooksPageComponent
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        WebhooksPageComponent
    ]
})
export class SqxFeatureWebhooksModule { }