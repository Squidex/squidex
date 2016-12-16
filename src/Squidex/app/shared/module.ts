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
    DashboardLinkDirective,
    HistoryComponent
} from './declarations';

@NgModule({
    imports: [
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        DashboardLinkDirective,
        HistoryComponent
    ],
    exports: [
        AppFormComponent,
        DashboardLinkDirective,
        HistoryComponent
    ]
})
export class SqxSharedModule { }