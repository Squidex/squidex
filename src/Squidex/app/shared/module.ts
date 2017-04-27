/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ModuleWithProviders, NgModule } from '@angular/core';
import { DndModule } from 'ng2-dnd';

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
    AssetComponent,
    AssetsEditorComponent,
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
    UserEmailPipe,
    UserEmailRefPipe,
    UserNamePipe,
    UserNameRefPipe,
    UserPicturePipe,
    UserPictureRefPipe,
    UserManagementService,
    UsersProviderService,
    UsersService
} from './declarations';

@NgModule({
    imports: [
        ProgressHttpModule,
        DndModule,
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AssetComponent,
        AssetsEditorComponent,
        DashboardLinkDirective,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        UserEmailPipe,
        UserEmailRefPipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe
    ],
    exports: [
        AppFormComponent,
        AssetComponent,
        AssetsEditorComponent,
        DashboardLinkDirective,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        UserEmailPipe,
        UserEmailRefPipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe
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