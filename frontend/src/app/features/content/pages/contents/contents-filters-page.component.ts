/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed } from '@angular/core';
import { ContentsState, LayoutComponent, Queries, Query, QueryListComponent, SavedQueriesComponent, SchemasState, TranslatePipe, UIState } from '@app/shared';

@Component({
    selector: 'sqx-contents-filters-page',
    styleUrls: ['./contents-filters-page.component.scss'],
    templateUrl: './contents-filters-page.component.html',
    imports: [
        LayoutComponent,
        QueryListComponent,
        SavedQueriesComponent,
        TranslatePipe,
    ],
})
export class ContentsFiltersPageComponent {
    protected readonly schemaQueries = computed(() => {
        const schema = this.schemasState.selectedSchema();
        if (!schema) {
            return null;
        }

        return new Queries(this.uiState, `schemas.${schema.name}`);
    });

    constructor(
        protected readonly contentsState: ContentsState,
        private readonly schemasState: SchemasState,
        private readonly uiState: UIState,
    ) {
    }

    protected search(query?: Query) {
        this.contentsState.search(query);
    }
}
