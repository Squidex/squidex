/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import {
    AssetsState,
    DialogModel,
    LocalStoreService,
    Queries,
    Query,
    ResourceOwner,
    UIState
} from '@app/shared';

@Component({
    selector: 'sqx-assets-page',
    styleUrls: ['./assets-page.component.scss'],
    templateUrl: './assets-page.component.html'
})
export class AssetsPageComponent extends ResourceOwner implements OnInit {
    public assetsFilter = new FormControl();

    public queries = new Queries(this.uiState, 'assets');

    public addAssetFolderDialog = new DialogModel();

    public isListView: boolean;

    constructor(
        public readonly assetsState: AssetsState,
        private readonly localStore: LocalStoreService,
        private readonly route: ActivatedRoute,
        private readonly uiState: UIState
    ) {
        super();

        this.isListView = this.localStore.getBoolean('squidex.assets.list-view');
    }

    public ngOnInit() {
        this.own(
            this.route.queryParams
                .subscribe(p => {
                    this.assetsState.search({ fullText: p['query'] });
                }));
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