/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { AssetDto, AssetsState, DialogModel, LocalStoreService, MathHelper, Queries, Query, QueryFullTextSynchronizer, Router2State, UIState } from '@app/shared';
import { Settings } from '@app/shared/state/settings';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html',
    providers: [
        Router2State,
    ],
})
export class AssetsPageComponent implements OnInit {
    public editAsset?: AssetDto;

    public listQueries = new Queries(this.uiState, 'assets');
    public listMode = false;

    public addAssetFolderDialog = new DialogModel();

    constructor(
        public readonly assetsRoute: Router2State,
        public readonly assetsState: AssetsState,
        private readonly localStore: LocalStoreService,
        private readonly uiState: UIState,
    ) {
        this.listMode = this.localStore.getBoolean(Settings.Local.ASSETS_MODE);
    }

    public ngOnInit() {
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
        this.editAsset = asset;
    }

    public editDone() {
        this.editAsset = undefined;
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
        this.listMode = isListView;

        this.localStore.setBoolean(Settings.Local.ASSETS_MODE, isListView);
    }
}
