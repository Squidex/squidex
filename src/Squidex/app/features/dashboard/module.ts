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
    DashboardPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: DashboardPageComponent
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        DashboardPageComponent
    ]
})
export class SqxFeatureDashboardModule { }