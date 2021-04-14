﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { DragDropModule } from '@angular/cdk/drag-drop';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SqxFrameworkModule } from '@app/framework';
import { MentionModule } from 'angular-mentions';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { AssetFolderDropdownComponent } from './components/assets/asset-folder-dropdown.component';
import { PreviewableType } from './components/assets/pipes';
import { AppFormComponent, AppLanguagesService, AppMustExistGuard, AppsService, AppsState, AssetComponent, AssetDialogComponent, AssetFolderComponent, AssetFolderDialogComponent, AssetHistoryComponent, AssetPathComponent, AssetPreviewUrlPipe, AssetsListComponent, AssetsSelectorComponent, AssetsService, AssetsState, AssetTextEditorComponent, AssetUploaderComponent, AssetUploaderState, AssetUrlPipe, AuthInterceptor, AuthService, AutoSaveService, BackupsService, BackupsState, ClientsService, ClientsState, CommentComponent, CommentsComponent, CommentsService, ContentMustExistGuard, ContentsService, ContentsState, ContributorsService, ContributorsState, FileIconPipe, FilterComparisonComponent, FilterLogicalComponent, FilterNodeComponent, GeolocationEditorComponent, GraphQlService, HelpComponent, HelpMarkdownPipe, HelpService, HistoryComponent, HistoryListComponent, HistoryMessagePipe, HistoryService, ImageCropperComponent, ImageFocusPointComponent, LanguagesService, LanguagesState, LoadAppsGuard, LoadLanguagesGuard, MarkdownEditorComponent, MustBeAuthenticatedGuard, MustBeNotAuthenticatedGuard, NewsService, NotifoComponent, PlansService, PlansState, QueryComponent, QueryListComponent, QueryPathComponent, ReferencesCheckboxesComponent, ReferencesDropdownComponent, ReferencesTagsComponent, RichEditorComponent, RolesService, RolesState, RuleEventsState, RulesService, RulesState, SavedQueriesComponent, SchemaCategoryComponent, SchemaMustExistGuard, SchemaMustExistPublishedGuard, SchemaMustNotBeSingletonGuard, SchemasService, SchemasState, SchemaTagSource, SearchFormComponent, SortingComponent, StockPhotoService, TableHeaderComponent, TranslationsService, UIService, UIState, UnsetAppGuard, UnsetContentGuard, UsagesService, UserDtoPicture, UserIdPicturePipe, UserNamePipe, UserNameRefPipe, UserPicturePipe, UserPictureRefPipe, UsersProviderService, UsersService, WorkflowsService, WorkflowsState } from './declarations';
import { SearchService } from './services/search.service';

@NgModule({
    imports: [
        DragDropModule,
        MentionModule,
        NgxDocViewerModule,
        RouterModule,
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AssetComponent,
        AssetDialogComponent,
        AssetFolderComponent,
        AssetFolderDialogComponent,
        AssetFolderDropdownComponent,
        AssetHistoryComponent,
        AssetPathComponent,
        AssetPreviewUrlPipe,
        AssetsListComponent,
        AssetsSelectorComponent,
        AssetTextEditorComponent,
        AssetUploaderComponent,
        AssetUrlPipe,
        CommentComponent,
        CommentsComponent,
        FileIconPipe,
        FilterComparisonComponent,
        FilterLogicalComponent,
        FilterNodeComponent,
        GeolocationEditorComponent,
        HelpComponent,
        HelpMarkdownPipe,
        HistoryComponent,
        HistoryListComponent,
        HistoryMessagePipe,
        ImageCropperComponent,
        ImageFocusPointComponent,
        MarkdownEditorComponent,
        NotifoComponent,
        PreviewableType,
        QueryComponent,
        QueryListComponent,
        QueryPathComponent,
        ReferencesCheckboxesComponent,
        ReferencesDropdownComponent,
        ReferencesTagsComponent,
        RichEditorComponent,
        SavedQueriesComponent,
        SchemaCategoryComponent,
        SearchFormComponent,
        SortingComponent,
        TableHeaderComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe
    ],
    exports: [
        AppFormComponent,
        AssetComponent,
        AssetDialogComponent,
        AssetFolderComponent,
        AssetFolderDialogComponent,
        AssetFolderDropdownComponent,
        AssetPathComponent,
        AssetPreviewUrlPipe,
        AssetsListComponent,
        AssetsSelectorComponent,
        AssetUploaderComponent,
        AssetUrlPipe,
        CommentComponent,
        CommentsComponent,
        DragDropModule,
        FileIconPipe,
        GeolocationEditorComponent,
        HelpComponent,
        HelpMarkdownPipe,
        HistoryComponent,
        HistoryListComponent,
        HistoryMessagePipe,
        MarkdownEditorComponent,
        NotifoComponent,
        PreviewableType,
        QueryListComponent,
        ReferencesCheckboxesComponent,
        ReferencesDropdownComponent,
        ReferencesTagsComponent,
        RichEditorComponent,
        RouterModule,
        SavedQueriesComponent,
        SchemaCategoryComponent,
        SearchFormComponent,
        TableHeaderComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe
    ]
})
export class SqxSharedModule {
    public static forRoot(): ModuleWithProviders<SqxSharedModule> {
        return {
            ngModule: SqxSharedModule,
            providers: [
                AppLanguagesService,
                AppMustExistGuard,
                AppsService,
                AppsState,
                AssetsService,
                AssetsState,
                AssetUploaderState,
                AuthService,
                AutoSaveService,
                BackupsService,
                BackupsState,
                ClientsService,
                ClientsState,
                CommentsService,
                ContentMustExistGuard,
                ContentsService,
                ContentsState,
                ContributorsService,
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
                PlansService,
                PlansState,
                RolesService,
                RolesState,
                RuleEventsState,
                RulesService,
                RulesState,
                SchemaMustExistGuard,
                SchemaMustExistPublishedGuard,
                SchemaMustNotBeSingletonGuard,
                SchemasService,
                SchemasState,
                SchemaTagSource,
                SearchService,
                StockPhotoService,
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