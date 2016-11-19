/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Ng2CompleterModule } from 'ng2-completer';

import { SqxFrameworkModule } from 'shared';
import { SqxLayoutModule } from 'components/layout';

import {
    AppAreaComponent,
    AppsPageComponent,
    ContributorsPageComponent,
    CredentialsPageComponent,
    DashboardPageComponent,
    InternalAreaComponent,
    LeftMenuComponent,
    LanguagesPageComponent,
    SchemasPageComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        Ng2CompleterModule,
        SqxFrameworkModule,
        SqxLayoutModule
    ],
    exports: [
        LeftMenuComponent
    ],
    declarations: [
        AppAreaComponent,
        AppsPageComponent,
        ContributorsPageComponent,
        CredentialsPageComponent,
        DashboardPageComponent,
        InternalAreaComponent,
        LanguagesPageComponent,
        LeftMenuComponent,
        SchemasPageComponent
    ]
})
export class SqxAppModule { }