/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { DndModule } from 'ng2-dnd';

import { SqxFrameworkModule } from 'framework';

import {
    AppFormComponent,
    AppClientsService,
    AppContributorsService,
    AppLanguagesService,
    AppMustExistGuard,
    AppPatternsService,
    AppsStoreService,
    AppsService,
    AssetComponent,
    AssetPreviewUrlPipe,
    AssetsService,
    AssetUrlPipe,
    AuthInterceptor,
    AuthService,
    BackupsService,
    ContentsService,
    EventConsumersService,
    FileIconPipe,
    GeolocationEditorComponent,
    GraphQlService,
    HelpComponent,
    HelpService,
    HistoryComponent,
    HistoryService,
    LanguageSelectorComponent,
    LanguagesService,
    MarkdownEditorComponent,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    PlansService,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    ResolvePublishedSchemaGuard,
    ResolveSchemaGuard,
    SchemasService,
    ResolveUserGuard,
    RulesService,
    UIService,
    UnsetAppGuard,
    UsagesService,
    UserDtoPicture,
    UserNamePipe,
    UserNameRefPipe,
    UserIdPicturePipe,
    UserPicturePipe,
    UserPictureRefPipe,
    UserManagementService,
    UsersProviderService,
    UsersService,
    RichEditorComponent
} from './declarations';

@NgModule({
    imports: [
        DndModule,
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AssetComponent,
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        FileIconPipe,
        GeolocationEditorComponent,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        MarkdownEditorComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        RichEditorComponent
    ],
    exports: [
        AppFormComponent,
        AssetComponent,
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        FileIconPipe,
        GeolocationEditorComponent,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        MarkdownEditorComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        RichEditorComponent
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
                AppMustExistGuard,
                AppPatternsService,
                AppsService,
                AppsStoreService,
                AssetsService,
                AuthService,
                BackupsService,
                ContentsService,
                EventConsumersService,
                GraphQlService,
                HelpService,
                HistoryService,
                LanguagesService,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                PlansService,
                ResolveAppLanguagesGuard,
                ResolveContentGuard,
                ResolvePublishedSchemaGuard,
                ResolveSchemaGuard,
                ResolveUserGuard,
                RulesService,
                SchemasService,
                UIService,
                UnsetAppGuard,
                UsagesService,
                UserManagementService,
                UsersProviderService,
                UsersService,
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: AuthInterceptor,
                    multi: true
                }
            ]
        };
    }
}