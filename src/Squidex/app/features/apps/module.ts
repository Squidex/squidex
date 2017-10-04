/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SqxFrameworkModule, SqxSharedModule } from 'shared';

import {
    AppsPageComponent,
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
        OnboardingDialogComponent
    ]
})
export class SqxFeatureAppsModule { }