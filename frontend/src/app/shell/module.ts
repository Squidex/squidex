/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { SqxFrameworkModule, SqxSharedModule } from '@app/shared';
import { AppAreaComponent, AppsMenuComponent, ForbiddenPageComponent, HomePageComponent, InternalAreaComponent, LeftMenuComponent, LoginPageComponent, LogoComponent, LogoutPageComponent, NotFoundPageComponent, NotificationDropdownComponent, NotificationsMenuComponent, ProfileMenuComponent, SearchMenuComponent, TeamsAreaComponent } from './declarations';

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
    ],
    exports: [
        AppAreaComponent,
        HomePageComponent,
        ForbiddenPageComponent,
        InternalAreaComponent,
        NotFoundPageComponent,
    ],
    declarations: [
        AppAreaComponent,
        AppsMenuComponent,
        ForbiddenPageComponent,
        HomePageComponent,
        InternalAreaComponent,
        LeftMenuComponent,
        LoginPageComponent,
        LogoComponent,
        LogoutPageComponent,
        NotFoundPageComponent,
        NotificationDropdownComponent,
        NotificationsMenuComponent,
        ProfileMenuComponent,
        SearchMenuComponent,
        TeamsAreaComponent,
    ],
})
export class SqxShellModule { }
