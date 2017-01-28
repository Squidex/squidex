/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';

import { SqxFrameworkModule, SqxSharedModule } from 'shared';

import {
    AppAreaComponent,
    AppsMenuComponent,
    HomePageComponent,
    InternalAreaComponent,
    LeftMenuComponent,
    LogoutPageComponent,
    NotFoundPageComponent,
    ProfileMenuComponent
} from './declarations';

@NgModule({
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
        ProfileMenuComponent
    ]
})
export class SqxShellModule { }