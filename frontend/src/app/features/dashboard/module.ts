/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { GridsterModule } from 'angular-gridster2';
import { ChartModule } from 'angular2-chartjs';
import { ApiCallsCardComponent, ApiCallsSummaryCardComponent, ApiCardComponent, ApiPerformanceCardComponent, ApiTrafficCardComponent, ApiTrafficSummaryCardComponent, AssetUploadsCountCardComponent, AssetUploadsSizeCardComponent, AssetUploadsSizeSummaryCardComponent, ContentSummaryCardComponent, DashboardConfigComponent, DashboardPageComponent, GithubCardComponent, HistoryCardComponent, IFrameCardComponent, RandomCatCardComponent, RandomDogCardComponent, SchemaCardComponent, SupportCardComponent } from './declarations';

const routes: Routes = [
    {
        path: '',
        component: DashboardPageComponent,
    },
];

@NgModule({
    imports: [
        ChartModule,
        GridsterModule,
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    declarations: [
        ApiCallsCardComponent,
        ApiCallsSummaryCardComponent,
        ApiCardComponent,
        ApiPerformanceCardComponent,
        ApiTrafficCardComponent,
        ApiTrafficSummaryCardComponent,
        AssetUploadsCountCardComponent,
        AssetUploadsSizeCardComponent,
        AssetUploadsSizeSummaryCardComponent,
        ContentSummaryCardComponent,
        DashboardConfigComponent,
        DashboardPageComponent,
        GithubCardComponent,
        HistoryCardComponent,
        IFrameCardComponent,
        RandomCatCardComponent,
        RandomDogCardComponent,
        SchemaCardComponent,
        SupportCardComponent,
    ],
})
export class SqxFeatureDashboardModule {}
