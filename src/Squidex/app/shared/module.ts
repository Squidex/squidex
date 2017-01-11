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
    DashboardLinkDirective,
    HistoryComponent,
    HistoryService,
    LanguageService,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
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
        HistoryComponent
    ],
    exports: [
        AppFormComponent,
        DashboardLinkDirective,
        HistoryComponent
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
                HistoryService,
                LanguageService,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                ResolvePublishedSchemaGuard,
                ResolveSchemaGuard,
                SchemasService,
                UsersProviderService,
                UsersService
            ]
        };
    }
}