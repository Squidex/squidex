/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AssetDialogComponent, AssetDto, AssetFolderDialogComponent, AssetPathComponent, AssetsListComponent, AssetsState, DialogModel, LayoutComponent, ListViewComponent, LocalStoreService, MathHelper, ModalDirective, PagerComponent, Queries, Query, QueryFullTextSynchronizer, Router2State, SearchFormComponent, Settings, ShortcutDirective, SidebarMenuDirective, TagEditorComponent, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, UIState } from '@app/shared';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html',
    providers: [
        Router2State,
    ],
    imports: [
        AssetDialogComponent,
        AssetFolderDialogComponent,
        AssetPathComponent,
        AssetsListComponent,
        AsyncPipe,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        ModalDirective,
        PagerComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SearchFormComponent,
        ShortcutDirective,
        SidebarMenuDirective,
        TagEditorComponent,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AssetsPageComponent {
    public readonly editAsset = signal<AssetDto | undefined>(undefined);

    public readonly listQueries = new Queries(this.uiState, 'assets');
    public readonly listMode = signal(this.localStore.getBoolean(Settings.Local.ASSETS_MODE));

    public readonly addAssetFolderDialog = new DialogModel();

    constructor(
        public readonly assetsRoute: Router2State,
        public readonly assetsState: AssetsState,
        private readonly localStore: LocalStoreService,
        private readonly uiState: UIState,
    ) {
        const initial =
            this.assetsRoute.mapTo(this.assetsState)
                .withPaging('assets', 30)
                .withString('ref')
                .withStringOr('parentId', MathHelper.EMPTY_GUID)
                .withStrings('tagsSelected')
                .withSynchronizer(QueryFullTextSynchronizer.INSTANCE)
                .getInitial();

        this.assetsState.load(false, true, initial);
        this.assetsRoute.listen();
    }

    public reload() {
        this.assetsState.load(true);
    }

    public reloadTotal() {
        this.assetsState.load(true, false);
    }

    public editStart(asset: AssetDto) {
        this.editAsset.set(asset);
    }

    public editDone() {
        this.editAsset.set(undefined);
    }

    public search(query: Query) {
        this.assetsState.search(query);
    }

    public replaceAsset(asset: AssetDto) {
        this.assetsState.replaceAsset(asset);
    }

    public selectTags(tags: ReadonlyArray<string>) {
        this.assetsState.selectTags(tags);
    }

    public toggleTag(tag: string) {
        this.assetsState.toggleTag(tag);
    }

    public changeView(isListView: boolean) {
        this.listMode.set(isListView);

        this.localStore.setBoolean(Settings.Local.ASSETS_MODE, isListView);
    }
}
