/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { ContentsState, defined, Queries, Query, SchemasState, UIState } from '@app/shared';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-contents-filters-page',
    styleUrls: ['./contents-filters-page.component.scss'],
    templateUrl: './contents-filters-page.component.html',
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
