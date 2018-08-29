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
    AppClientsService,
    AppContributorsService,
    AppFormComponent,
    AppLanguagesService,
    AppMustExistGuard,
    AppPatternsService,
    AppsService,
    AppsState,
    AssetComponent,
    AssetPreviewUrlPipe,
    AssetsDialogState,
    AssetsListComponent,
    AssetsSelectorComponent,
    AssetsService,
    AssetsState,
    AssetUrlPipe,
    AuthInterceptor,
    AuthService,
    BackupsService,
    BackupsState,
    ClientsState,
    ContentMustExistGuard,
    ContentsService,
    ContentsState,
    ContributorsState,
    FileIconPipe,
    GeolocationEditorComponent,
    GraphQlService,
    HelpComponent,
    HelpService,
    HistoryComponent,
    HistoryListComponent,
    HistoryService,
    LanguageSelectorComponent,
    LanguagesService,
    LanguagesState,
    LoadAppsGuard,
    LoadLanguagesGuard,
    MarkdownEditorComponent,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    PatternsState,
    PlansService,
    PlansState,
    RichEditorComponent,
    RuleEventsState,
    RulesService,
    RulesState,
    SchemaCategoryComponent,
    SchemaMustExistGuard,
    SchemaMustExistPublishedGuard,
    SchemasService,
    SchemasState,
    SearchFormComponent,
    UIService,
    UIState,
    UnsetAppGuard,
    UnsetContentGuard,
    UsagesService,
    UserDtoPicture,
    UserIdPicturePipe,
    UserNamePipe,
    UserNameRefPipe,
    UserPicturePipe,
    UserPictureRefPipe,
    UsersProviderService,
    UsersService
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
        HistoryListComponent,
        LanguageSelectorComponent,
        MarkdownEditorComponent,
        SchemaCategoryComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        RichEditorComponent,
        SearchFormComponent
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
        HistoryListComponent,
        LanguageSelectorComponent,
        MarkdownEditorComponent,
        RouterModule,
        SchemaCategoryComponent,
        SearchFormComponent,
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
                BackupsState,
                ClientsState,
                ContentMustExistGuard,
                ContentsService,
                ContentsState,
                ContributorsState,
                GraphQlService,
                HelpService,
                HistoryService,
                LanguagesService,
                LanguagesState,
                LoadAppsGuard,
                LoadLanguagesGuard,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                PatternsState,
                PlansService,
                PlansState,
                RuleEventsState,
                RulesService,
                RulesState,
                SchemaMustExistGuard,
                SchemaMustExistPublishedGuard,
                SchemasService,
                SchemasState,
                UIService,
                UIState,
                UnsetAppGuard,
                UnsetContentGuard,
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