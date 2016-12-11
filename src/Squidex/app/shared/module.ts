/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { SqxFrameworkModule } from 'shared';

import {
    AppFormComponent,
    DashboardLinkDirective
} from './declarations';

@Ng2.NgModule({
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