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
    ClientComponent,
    ClientsPageComponent,
    ContributorsPageComponent,
    DashboardPageComponent,
    InternalAreaComponent,
    LeftMenuComponent,
    LanguagesPageComponent,
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
        ClientComponent,
        ClientsPageComponent,
        ContributorsPageComponent,
        DashboardPageComponent,
        InternalAreaComponent,
        LanguagesPageComponent,
        LeftMenuComponent,
        SchemasPageComponent
    ]
})
export class SqxAppModule { }