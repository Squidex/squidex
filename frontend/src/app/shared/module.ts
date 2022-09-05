/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DragDropModule } from '@angular/cdk/drag-drop';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MentionModule } from 'angular-mentions';
import { NgxDocViewerModule } from 'ngx-doc-viewer';
import { SqxFrameworkModule } from '@app/framework';
import { AppFormComponent, AppLanguagesService, AppMustExistGuard, AppsService, AppsState, AssetComponent, AssetDialogComponent, AssetFolderComponent, AssetFolderDialogComponent, AssetFolderDropdownComponent, AssetFolderDropdownItemComponent, AssetHistoryComponent, AssetPathComponent, AssetPreviewUrlPipe, AssetScriptsState, AssetsListComponent, AssetsSelectorComponent, AssetsService, AssetsState, AssetTextEditorComponent, AssetUploaderComponent, AssetUploaderState, AssetUrlPipe, AuthInterceptor, AuthService, AutoSaveService, BackupsService, BackupsState, ClientsService, ClientsState, CommentComponent, CommentsComponent, CommentsService, ContentListCellDirective, ContentListCellResizeDirective, ContentListFieldComponent, ContentListHeaderComponent, ContentListWidthDirective, ContentMustExistGuard, ContentsColumnsPipe, ContentSelectorComponent, ContentSelectorItemComponent, ContentsService, ContentsState, ContentStatusComponent, ContentValueComponent, ContentValueEditorComponent, ContributorsService, ContributorsState, FileIconPipe, FilterComparisonComponent, FilterLogicalComponent, FilterNodeComponent, FilterOperatorPipe, GeolocationEditorComponent, HelpComponent, HelpMarkdownPipe, HelpService, HistoryComponent, HistoryListComponent, HistoryMessagePipe, HistoryService, ImageCropperComponent, ImageFocusPointComponent, LanguagesService, LanguagesState, LoadAppsGuard, LoadLanguagesGuard, LoadSchemasGuard, MarkdownEditorComponent, MustBeAuthenticatedGuard, MustBeNotAuthenticatedGuard, NewsService, NotifoComponent, PlansService, PlansState, PreviewableType, QueryComponent, QueryListComponent, QueryPathComponent, ReferenceInputComponent, RichEditorComponent, RolesService, RolesState, RuleEventsState, RuleMustExistGuard, RuleSimulatorState, RulesService, RulesState, SavedQueriesComponent, SchemaCategoryComponent, SchemaMustExistGuard, SchemaMustExistPublishedGuard, SchemaMustNotBeSingletonGuard, SchemasService, SchemasState, SchemaTagSource, SearchFormComponent, SearchService, SortingComponent, StockPhotoService, TableHeaderComponent, TemplatesService, TemplatesState, TranslationsService, TranslationStatusComponent, UIService, UIState, UnsetAppGuard, UsagesService, UserDtoPicture, UserIdPicturePipe, UserNamePipe, UserNameRefPipe, UserPicturePipe, UserPictureRefPipe, UsersProviderService, UsersService, WatchingUsersComponent, WorkflowsService, WorkflowsState } from './declarations';

@NgModule({
    imports: [
        DragDropModule,
        MentionModule,
        NgxDocViewerModule,
        RouterModule,
        SqxFrameworkModule,
    ],
    declarations: [
        AppFormComponent,
        AssetComponent,
        AssetDialogComponent,
        AssetFolderComponent,
        AssetFolderDialogComponent,
        AssetFolderDropdownComponent,
        AssetFolderDropdownItemComponent,
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
        ContentListCellDirective,
        ContentListCellResizeDirective,
        ContentListFieldComponent,
        ContentListHeaderComponent,
        ContentListWidthDirective,
        ContentsColumnsPipe,
        ContentSelectorComponent,
        ContentSelectorItemComponent,
        ContentStatusComponent,
        ContentValueComponent,
        ContentValueEditorComponent,
        FileIconPipe,
        FilterComparisonComponent,
        FilterLogicalComponent,
        FilterNodeComponent,
        FilterOperatorPipe,
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
        ReferenceInputComponent,
        RichEditorComponent,
        SavedQueriesComponent,
        SchemaCategoryComponent,
        SearchFormComponent,
        SortingComponent,
        TableHeaderComponent,
        TranslationStatusComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        WatchingUsersComponent,
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
        ContentListCellDirective,
        ContentListCellResizeDirective,
        ContentListFieldComponent,
        ContentListHeaderComponent,
        ContentListWidthDirective,
        ContentsColumnsPipe,
        ContentSelectorComponent,
        ContentSelectorItemComponent,
        ContentStatusComponent,
        ContentValueComponent,
        ContentValueEditorComponent,
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
        ReferenceInputComponent,
        RichEditorComponent,
        RouterModule,
        SavedQueriesComponent,
        SchemaCategoryComponent,
        SearchFormComponent,
        TableHeaderComponent,
        TranslationStatusComponent,
        UserDtoPicture,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe,
        WatchingUsersComponent,
    ],
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
                AssetScriptsState,
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
                HelpService,
                HistoryService,
                LanguagesService,
                LanguagesState,
                LoadAppsGuard,
                LoadLanguagesGuard,
                LoadSchemasGuard,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                NewsService,
                PlansService,
                PlansState,
                RolesService,
                RolesState,
                RuleEventsState,
                RuleMustExistGuard,
                RuleSimulatorState,
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
                TemplatesService,
                TemplatesState,
                TranslationsService,
                UIService,
                UIState,
                UnsetAppGuard,
                UsagesService,
                UsersProviderService,
                UsersService,
                WorkflowsService,
                WorkflowsState,
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: AuthInterceptor,
                    multi: true,
                },
            ],
        };
    }
}
