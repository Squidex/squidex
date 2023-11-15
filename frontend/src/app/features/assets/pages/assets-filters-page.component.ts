/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { AssetsState, LayoutComponent, Queries, Query, SavedQueriesComponent, TranslatePipe, UIState } from '@app/shared';
import { AssetTagsComponent } from './asset-tags.component';

@Component({
    selector: 'sqx-assets-filters-page',
    styleUrls: ['./assets-filters-page.component.scss'],
    templateUrl: './assets-filters-page.component.html',
    standalone: true,
    imports: [
        LayoutComponent,
        AssetTagsComponent,
        SavedQueriesComponent,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class AssetsFiltersPageComponent {
    public assetsQueries: Queries;

    constructor(uiState: UIState,
        public readonly assetsState: AssetsState,
    ) {
        this.assetsQueries = new Queries(uiState, 'assets');
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
