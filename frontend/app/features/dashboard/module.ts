/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChartModule } from 'angular2-chartjs';

import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';

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
        ChartModule,
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        DashboardPageComponent
    ]
})
export class SqxFeatureDashboardModule {}