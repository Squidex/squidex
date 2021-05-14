/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AppComponent, AppsPageComponent, NewsDialogComponent, OnboardingDialogComponent } from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AppsPageComponent,
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    declarations: [
        AppComponent,
        AppsPageComponent,
        NewsDialogComponent,
        OnboardingDialogComponent,
    ],
})
export class SqxFeatureAppsModule {}
