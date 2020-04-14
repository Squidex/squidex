/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { ChartModule } from 'angular2-chartjs';
import { DashboardPageComponent } from './declarations';

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