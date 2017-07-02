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
    AssetsService,
    AuthService,
    ContentsService,
    EventConsumersService,
    HelpComponent,
    HelpService,
    HistoryComponent,
    HistoryService,
    LanguageSelectorComponent,
    LanguageService,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    PlansService,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    ResolvePublishedSchemaGuard,
    ResolveSchemaGuard,
    SchemasService,
    ResolveUserGuard,
    UsagesService,
    UserDtoPicture,
    UserEmailPipe,
    UserEmailRefPipe,
    UserNamePipe,
    UserNameRefPipe,
    UserIdPicturePipe,
    UserPicturePipe,
    UserPictureRefPipe,
    UserManagementService,
    UsersProviderService,
    UsersService,
    WebhooksService
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
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        UserDtoPicture,
        UserEmailPipe,
        UserEmailRefPipe,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe
    ],
    exports: [
        AppFormComponent,
        AssetComponent,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        UserDtoPicture,
        UserEmailPipe,
        UserEmailRefPipe,
        UserIdPicturePipe,
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
                PlansService,
                ResolveAppLanguagesGuard,
                ResolveContentGuard,
                ResolvePublishedSchemaGuard,
                ResolveSchemaGuard,
                ResolveUserGuard,
                SchemasService,
                UsagesService,
                UserManagementService,
                UsersProviderService,
                UsersService,
                WebhooksService
            ]
        };
    }
}