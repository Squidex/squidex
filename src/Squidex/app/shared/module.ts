/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ModuleWithProviders, NgModule } from '@angular/core';

import { SqxFrameworkModule } from 'framework';

import {
    AppFormComponent,
    AppClientsService,
    AppContributorsService,
    AppLanguagesService,
    AppsStoreService,
    AppsService,
    AppMustExistGuard,
    AuthService,
    ContentsService,
    DashboardLinkDirective,
    EventConsumersService,
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
    UsersProviderService,
    UsersService
} from './declarations';

@NgModule({
    imports: [
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        DashboardLinkDirective,
        HistoryComponent,
        LanguageSelectorComponent
    ],
    exports: [
        AppFormComponent,
        DashboardLinkDirective,
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
                AuthService,
                ContentsService,
                EventConsumersService,
                HistoryService,
                LanguageService,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                ResolveAppLanguagesGuard,
                ResolveContentGuard,
                ResolvePublishedSchemaGuard,
                ResolveSchemaGuard,
                SchemasService,
                UsersProviderService,
                UsersService
            ]
        };
    }
}