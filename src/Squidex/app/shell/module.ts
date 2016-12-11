/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { SqxFrameworkModule, SqxSharedModule } from 'shared';

import {
    AppAreaComponent,
    AppsMenuComponent,
    HomePageComponent,
    InternalAreaComponent,
    LeftMenuComponent,
    LogoutPageComponent,
    NotFoundPageComponent,
    ProfileMenuComponent,
    SearchFormComponent
} from './declarations';

@Ng2.NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule
    ],
    exports: [
        AppAreaComponent,
        HomePageComponent,
        InternalAreaComponent,
        NotFoundPageComponent
    ],
    declarations: [
        AppAreaComponent,
        AppsMenuComponent,
        HomePageComponent,
        InternalAreaComponent,
        LeftMenuComponent,
        LogoutPageComponent,
        NotFoundPageComponent,
        ProfileMenuComponent,
        SearchFormComponent
    ]
})
export class SqxShellModule { }