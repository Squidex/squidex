/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AppsState,
    AssetsState,
    FilterState,
    LocalStoreService,
    Queries,
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

    public filter = new FilterState();

    public isListView: boolean;

    constructor(
        public readonly appsState: AppsState,
        public readonly assetsState: AssetsState,
        private readonly localStore: LocalStoreService,
        private readonly uiState: UIState
    ) {
        super();

        this.isListView = this.localStore.getBoolean('squidex.assets.list-view');
    }

    public ngOnInit() {
        this.assetsState.load().pipe(onErrorResumeNext()).subscribe();

        this.own(
            this.assetsState.assetsQuery
                .subscribe(query => {
                    this.filter.setQuery(query);
                }));
    }

    public reload() {
        this.assetsState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public search() {
        this.assetsState.search(this.filter.apiFilter).pipe(onErrorResumeNext()).subscribe();
    }

    public selectTags(tags: string[]) {
        this.assetsState.selectTags(tags).pipe(onErrorResumeNext()).subscribe();
    }

    public toggleTag(tag: string) {
        this.assetsState.toggleTag(tag).pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.assetsState.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.assetsState.goPrev().pipe(onErrorResumeNext()).subscribe();
    }

    public changeView(isListView: boolean) {
        this.isListView = isListView;

        this.localStore.setBoolean('squidex.assets.list-view', isListView);
    }
}

