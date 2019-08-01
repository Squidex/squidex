/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

import {
    AssetsState,
    Queries,
    Query,
    SavedQuery,
    UIState
} from '@app/shared';

@Component({
    selector: 'sqx-assets-filters-page',
    styleUrls: ['./assets-filters-page.component.scss'],
    templateUrl: './assets-filters-page.component.html'
})
export class AssetsFiltersPageComponent {
    public queries = new Queries(this.uiState, 'assets');

    constructor(
        public readonly assetsState: AssetsState,
        private readonly uiState: UIState
    ) {
    }

    public search(query: Query) {
        this.assetsState.search(query);
    }

    public selectTags(tags: string[]) {
        this.assetsState.selectTags(tags);
    }

    public toggleTag(tag: string) {
        this.assetsState.toggleTag(tag);
    }

    public resetTags() {
        this.assetsState.resetTags();
    }

    public isSelectedQuery(query: SavedQuery) {
        return this.assetsState.snapshot.assetsQueryJson === query.queryJson;
    }

    public trackByTag(index: number, tag: { name: string }) {
        return tag.name;
    }

    public trackByQuery(index: number, query: { name: string }) {
        return query.name;
    }
}