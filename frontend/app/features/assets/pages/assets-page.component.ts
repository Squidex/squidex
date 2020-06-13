/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { FormControl } from '@angular/forms';
import { AssetsState, DialogModel, LocalStoreService, Queries, Query, QueryFullTextSynchronizer, ResourceOwner, Router2State, UIState } from '@app/shared';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html',
    providers: [
        Router2State
    ]
})
export class AssetsPageComponent extends ResourceOwner {
    public assetsFilter = new FormControl();

    public queries = new Queries(this.uiState, 'assets');

    public addAssetFolderDialog = new DialogModel();

    public isListView: boolean;

    constructor(
        public readonly assetsSync: Router2State,
        public readonly assetsState: AssetsState,
        private readonly localStore: LocalStoreService,
        private readonly uiState: UIState
    ) {
        super();

        assetsSync.map(assetsState)
            .withPager('assetsPager', 'assets', 20)
            .withString('parentId', 'parent')
            .withStrings('tagsSelected', 'tags')
            .withSynchronizer('assetsQuery', new QueryFullTextSynchronizer())
            .build();

        this.isListView = this.localStore.getBoolean('squidex.assets.list-view');
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

        this.localStore.setBoolean('squidex.assets.list-view', isListView);
    }
}