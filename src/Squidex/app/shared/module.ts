﻿/*
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
    AppLanguagesService,
    AppMustExistGuard,
    AppsService,
    AppsState,
    AssetComponent,
    AssetDialogComponent,
    AssetPreviewUrlPipe,
    AssetsDialogState,
    AssetsListComponent,
    AssetsSelectorComponent,
    AssetsService,
    AssetsState,
    AssetUploaderComponent,
    AssetUploaderState,
    AssetUrlPipe,
    AuthInterceptor,
    AuthService,
    BackupsService,
    BackupsState,
    ClientsService,
    ClientsState,
    CommentComponent,
    CommentsComponent,
    CommentsService,
    ContentMustExistGuard,
    ContentsService,
    ContentsState,
    ContributorsService,
    ContributorsState,
    FileIconPipe,
    FilterComparisonComponent,
    FilterLogicalComponent,
    FilterNodeComponent,
    GeolocationEditorComponent,
    GraphQlService,
    HelpComponent,
    HelpMarkdownPipe,
    HelpService,
    HistoryComponent,
    HistoryListComponent,
    HistoryMessagePipe,
    HistoryService,
    LanguageSelectorComponent,
    LanguagesService,
    LanguagesState,
    LoadAppsGuard,
    LoadLanguagesGuard,
    MarkdownEditorComponent,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    NewsService,
    PatternsService,
    PatternsState,
    PlansService,
    PlansState,
    QueryComponent,
    RichEditorComponent,
    RolesService,
    RolesState,
    RuleEventsState,
    RulesService,
    RulesState,
    SchemaCategoryComponent,
    SchemaMustExistGuard,
    SchemaMustExistPublishedGuard,
    SchemaMustNotBeSingletonGuard,
    SchemasService,
    SchemasState,
    SearchFormComponent,
    SortingComponent,
    TableHeaderComponent,
    TranslationsService,
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
    UsersService,
    WorkflowsService,
    WorkflowsState
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
        AssetDialogComponent,
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        AssetsListComponent,
        AssetsSelectorComponent,
        AssetUploaderComponent,
        CommentComponent,
        CommentsComponent,
        FileIconPipe,
        FilterNodeComponent,
        FilterLogicalComponent,
        FilterComparisonComponent,
        GeolocationEditorComponent,
        HelpComponent,
        HelpMarkdownPipe,
        HistoryComponent,
        HistoryListComponent,
        HistoryMessagePipe,
        LanguageSelectorComponent,
        MarkdownEditorComponent,
        QueryComponent,
        SchemaCategoryComponent,
        SortingComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        RichEditorComponent,
        SearchFormComponent,
        TableHeaderComponent
    ],
    exports: [
        AppFormComponent,
        AssetComponent,
        AssetDialogComponent,
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        AssetsListComponent,
        AssetsSelectorComponent,
        AssetUploaderComponent,
        CommentComponent,
        CommentsComponent,
        FileIconPipe,
        GeolocationEditorComponent,
        HelpComponent,
        HelpMarkdownPipe,
        HistoryComponent,
        HistoryListComponent,
        HistoryMessagePipe,
        LanguageSelectorComponent,
        MarkdownEditorComponent,
        RichEditorComponent,
        RouterModule,
        SchemaCategoryComponent,
        SearchFormComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        TableHeaderComponent
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
                ClientsService,
                ContributorsService,
                AppLanguagesService,
                AppMustExistGuard,
                PatternsService,
                RolesService,
                AppsService,
                AppsState,
                AssetsState,
                AssetsService,
                AssetUploaderState,
                AuthService,
                BackupsService,
                BackupsState,
                ClientsState,
                CommentsService,
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
                NewsService,
                PatternsState,
                PlansService,
                PlansState,
                RolesState,
                RuleEventsState,
                RulesService,
                RulesState,
                SchemaMustExistGuard,
                SchemaMustExistPublishedGuard,
                SchemaMustNotBeSingletonGuard,
                SchemasService,
                SchemasState,
                TranslationsService,
                UIService,
                UIState,
                UnsetAppGuard,
                UnsetContentGuard,
                UsagesService,
                UsersProviderService,
                UsersService,
                WorkflowsService,
                WorkflowsState,
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: AuthInterceptor,
                    multi: true
                }
            ]
        };
    }
}