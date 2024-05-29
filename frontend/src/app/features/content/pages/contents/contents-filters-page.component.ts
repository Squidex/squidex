/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { map } from 'rxjs/operators';
import { ContentsState, defined, LayoutComponent, Queries, Query, QueryListComponent, SavedQueriesComponent, SchemasState, TranslatePipe, UIState } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-contents-filters-page',
    styleUrls: ['./contents-filters-page.component.scss'],
    templateUrl: './contents-filters-page.component.html',
    imports: [
        AsyncPipe,
        LayoutComponent,
        QueryListComponent,
        SavedQueriesComponent,
        TranslatePipe,
    ],
})
export class ContentsFiltersPageComponent {
    public schemaQueries =
        this.schemasState.selectedSchema.pipe(
            defined(),
            map(schema => new Queries(this.uiState, `schemas.${schema.name}`),
        ));

    constructor(
        public readonly contentsState: ContentsState,
        private readonly schemasState: SchemasState,
        private readonly uiState: UIState,
    ) {
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }
}
