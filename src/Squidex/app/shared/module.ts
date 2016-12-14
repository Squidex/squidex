/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';

import { SqxFrameworkModule } from 'framework';

import {
    AppFormComponent,
    DashboardLinkDirective
} from './declarations';

@NgModule({
    imports: [
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        DashboardLinkDirective
    ],
    exports: [
        AppFormComponent,
        DashboardLinkDirective
    ]
})
export class SqxSharedModule { }