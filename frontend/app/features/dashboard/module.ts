/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { GridsterModule } from 'angular-gridster2';
import { ChartModule } from 'angular2-chartjs';
import { ApiCallsCardComponent,  ApiCallsSummaryCardComponent, ApiCardComponent, ApiPerformanceCardComponent, ApiTrafficCardComponent, AssetUploadsCountCardComponent, AssetUploadsSizeCardComponent, AssetUploadsSizeSummaryCardComponent, DashboardPageComponent, GithubCardComponent, HistoryCardComponent, SchemaCardComponent, SupportCardComponent } from './declarations';

const routes: Routes = [
    {
        path: '',
        component: DashboardPageComponent
    }
];

@NgModule({
    imports: [
        ChartModule,
        GridsterModule,
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule
    ],
    declarations: [
        ApiCallsCardComponent,
        ApiCallsSummaryCardComponent,
        ApiCardComponent,
        ApiPerformanceCardComponent,
        ApiTrafficCardComponent,
        AssetUploadsCountCardComponent,
        AssetUploadsSizeCardComponent,
        AssetUploadsSizeSummaryCardComponent,
        DashboardPageComponent,
        GithubCardComponent,
        HistoryCardComponent,
        SchemaCardComponent,
        SupportCardComponent
    ]
})
export class SqxFeatureDashboardModule {}