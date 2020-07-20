/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { AssetsState, Queries, Query, UIState } from '@app/shared';

@Component({
    selector: 'sqx-assets-filters-page',
    styleUrls: ['./assets-filters-page.component.scss'],
    templateUrl: './assets-filters-page.component.html'
})
export class AssetsFiltersPageComponent {
    public assetsQueries = new Queries(this.uiState, 'assets');

    constructor(
        public readonly assetsState: AssetsState,
        private readonly uiState: UIState
    ) {
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

    public resetTags() {
        this.assetsState.resetTags();
    }

    public trackByTag(_index: number, tag: { name: string }) {
        return tag.name;
    }
}