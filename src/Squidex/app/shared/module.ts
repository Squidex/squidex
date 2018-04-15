/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import { SqxFrameworkModule } from '@app/framework';

import {
    AppFormComponent,
    AppClientsService,
    AppContributorsService,
    AppLanguagesService,
    AppMustExistGuard,
    AppPatternsService,
    AppsState,
    AppsService,
    AssetComponent,
    AssetPreviewUrlPipe,
    AssetsDialogState,
    AssetsListComponent,
    AssetsSelectorComponent,
    AssetsState,
    AssetsService,
    AssetUrlPipe,
    AuthInterceptor,
    AuthService,
    BackupsService,
    ContentsService,
    FileIconPipe,
    GeolocationEditorComponent,
    GraphQlService,
    HelpComponent,
    HelpService,
    HistoryComponent,
    HistoryService,
    LanguageSelectorComponent,
    LanguagesService,
    LoadAppsGuard,
    MarkdownEditorComponent,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    PlansService,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    SchemaMustExistGuard,
    SchemaMustExistPublishedGuard,
    SchemasService,
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
    UsersProviderService,
    UsersService,
    RichEditorComponent,
    SchemasState
} from './declarations';

@NgModule({
    imports: [
        DndModule,
        RouterModule,
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AssetComponent,
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        AssetsListComponent,
        AssetsSelectorComponent,
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
        AssetsListComponent,
        AssetsSelectorComponent,
        FileIconPipe,
        GeolocationEditorComponent,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        MarkdownEditorComponent,
        RouterModule,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        RichEditorComponent
    ],
    providers: [
        AssetsDialogState
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
                AppsState,
                AssetsState,
                AssetsService,
                AuthService,
                BackupsService,
                ContentsService,
                GraphQlService,
                HelpService,
                HistoryService,
                LanguagesService,
                LoadAppsGuard,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                PlansService,
                ResolveAppLanguagesGuard,
                ResolveContentGuard,
                RulesService,
                SchemaMustExistGuard,
                SchemaMustExistPublishedGuard,
                SchemasService,
                SchemasState,
                UIService,
                UnsetAppGuard,
                UsagesService,
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