/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, signal, inject } from '@angular/core';
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
    private readonly assetsRoute = inject(Router2State);
    private readonly localStore = inject(LocalStoreService);
    private readonly uiState = inject(UIState);

    protected readonly assetsState = inject(AssetsState);

    protected readonly editAsset = signal<AssetDto | undefined>(undefined);

    protected readonly listQueries = new Queries(this.uiState, 'assets');

    protected readonly listMode = signal(this.localStore.getBoolean(Settings.Local.ASSETS_MODE));

    protected readonly addAssetFolderDialog = new DialogModel();

    constructor() {
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

    protected reload() {
        this.assetsState.load(true);
    }

    protected reloadTotal() {
        this.assetsState.load(true, false);
    }

    protected editStart(asset: AssetDto) {
        this.editAsset.set(asset);
    }

    protected editDone() {
        this.editAsset.set(undefined);
    }

    protected search(query: Query) {
        this.assetsState.search(query);
    }

    protected replaceAsset(asset: AssetDto) {
        this.assetsState.replaceAsset(asset);
    }

    protected selectTags(tags: ReadonlyArray<string>) {
        this.assetsState.selectTags(tags);
    }

    protected changeView(isListView: boolean) {
        this.listMode.set(isListView);

        this.localStore.setBoolean(Settings.Local.ASSETS_MODE, isListView);
    }
}
