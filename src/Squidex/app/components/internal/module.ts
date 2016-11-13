/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { SqxFrameworkModule } from 'shared';
import { SqxLayoutModule } from 'components/layout';

import {
    AppAreaComponent,
    AppsPageComponent,
    DashboardPageComponent,
    InternalAreaComponent,
    LeftMenuComponent,
    SchemasPageComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule,
        SqxLayoutModule
    ],
    exports: [
        LeftMenuComponent
    ],
    declarations: [
        AppAreaComponent,
        AppsPageComponent,
        DashboardPageComponent,
        InternalAreaComponent,
        LeftMenuComponent,
        SchemasPageComponent
    ]
})
export class SqxAppModule { }