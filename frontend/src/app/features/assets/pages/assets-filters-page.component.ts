/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
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
    protected readonly assetsState = inject(AssetsState);

    protected readonly assetsQueries: Queries;

    constructor() {
        const uiState = inject(UIState);

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
