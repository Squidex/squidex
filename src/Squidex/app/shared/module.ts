/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ModuleWithProviders, NgModule } from '@angular/core';

import { ProgressHttpModule } from 'angular-progress-http';

import { SqxFrameworkModule } from 'framework';

import {
    AppFormComponent,
    AppClientsService,
    AppContributorsService,
    AppLanguagesService,
    AppsStoreService,
    AppsService,
    AppMustExistGuard,
    AssetsService,
    AuthService,
    ContentsService,
    DashboardLinkDirective,
    EventConsumersService,
    HelpComponent,
    HelpService,
    HistoryComponent,
    HistoryService,
    LanguageSelectorComponent,
    LanguageService,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    ResolvePublishedSchemaGuard,
    ResolveSchemaGuard,
    SchemasService,
    UserManagementService,
    UsersProviderService,
    UsersService
} from './declarations';

@NgModule({
    imports: [
        ProgressHttpModule,
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        DashboardLinkDirective,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent
    ],
    exports: [
        AppFormComponent,
        DashboardLinkDirective,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent
    ]
})
export class SqxSharedModule {
    public static forRoot(): ModuleWithProviders {
        return {
            ngModule: SqxSharedModule,
            providers: [
                AppClientsService,
                AppContributorsService,
                AppLanguagesService,
                AppsStoreService,
                AppsService,
                AppMustExistGuard,
                AssetsService,
                AuthService,
                ContentsService,
                EventConsumersService,
                HelpService,
                HistoryService,
                LanguageService,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                ResolveAppLanguagesGuard,
                ResolveContentGuard,
                ResolvePublishedSchemaGuard,
                ResolveSchemaGuard,
                SchemasService,
                UserManagementService,
                UsersProviderService,
                UsersService
            ]
        };
    }
}