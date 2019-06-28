/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';

import {
    AppsPageComponent,
    NewsDialogComponent,
    OnboardingDialogComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AppsPageComponent
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        AppsPageComponent,
        NewsDialogComponent,
        OnboardingDialogComponent
    ]
})
export class SqxFeatureAppsModule {}