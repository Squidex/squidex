/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { AssetsState, LayoutComponent, Queries, Query, SavedQueriesComponent, TranslatePipe, UIState } from '@app/shared';
import { AssetTagsComponent } from './asset-tags.component';

@Component({
    selector: 'sqx-assets-filters-page',
    styleUrls: ['./assets-filters-page.component.scss'],
    templateUrl: './assets-filters-page.component.html',
    imports: [
        AssetTagsComponent,
        LayoutComponent,
        SavedQueriesComponent,
        TranslatePipe,
    ],
})
export class AssetsFiltersPageComponent {
    protected readonly assetsQueries: Queries;

    constructor(uiState: UIState,
        protected readonly assetsState: AssetsState,
    ) {
        this.assetsQueries = new Queries(uiState, 'assets');
    }

    protected search(query?: Query) {
        this.assetsState.search(query);
    }

    protected toggleTag(tag: string) {
        this.assetsState.toggleTag(tag);
    }

    protected resetTags() {
        this.assetsState.resetTags();
    }
}
