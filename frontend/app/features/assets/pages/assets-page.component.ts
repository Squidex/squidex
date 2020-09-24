/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { AssetsState, DialogModel, LocalStoreService, Queries, Query, ResourceOwner, Router2State, UIState } from '@app/shared';
import { Settings } from '@app/shared/state/settings';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html',
    providers: [
        Router2State
    ]
})
export class AssetsPageComponent extends ResourceOwner implements OnInit {
    public queries = new Queries(this.uiState, 'assets');

    public addAssetFolderDialog = new DialogModel();

    public isListView: boolean;

    constructor(
        public readonly assetsRoute: Router2State,
        public readonly assetsState: AssetsState,
        private readonly localStore: LocalStoreService,
        private readonly uiState: UIState
    ) {
        super();

        this.isListView = this.localStore.getBoolean(Settings.Local.ASSETS_MODE);
    }

    public ngOnInit() {
        this.assetsState.loadAndListen(this.assetsRoute);
    }

    public reload() {
        this.assetsState.load(true);
    }

    public search(query: Query) {
        this.assetsState.search(query);
    }

    public selectTags(tags: ReadonlyArray<string>) {
        this.assetsState.selectTags(tags);
    }

    public toggleTag(tag: string) {
        this.assetsState.toggleTag(tag);
    }

    public changeView(isListView: boolean) {
        this.isListView = isListView;

        this.localStore.setBoolean(Settings.Local.ASSETS_MODE, isListView);
    }
}