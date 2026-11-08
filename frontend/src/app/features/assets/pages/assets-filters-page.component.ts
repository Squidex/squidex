/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AssetsState, LayoutComponent, Queries, Query, SavedQueriesComponent, TranslatePipe, UIState } from '@app/shared';
import { AssetTagsComponent } from './asset-tags.component';

@Component({
    selector: 'sqx-assets-filters-page',
    styleUrls: ['./assets-filters-page.component.scss'],
    templateUrl: './assets-filters-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetTagsComponent,
        LayoutComponent,
        SavedQueriesComponent,
        TranslatePipe,
    ],
})
export class AssetsFiltersPageComponent {
    private readonly uIState = inject(UIState);

    protected readonly assetsState = inject(AssetsState);

    protected readonly assetsQueries = new Queries(this.uIState, 'assets');

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
