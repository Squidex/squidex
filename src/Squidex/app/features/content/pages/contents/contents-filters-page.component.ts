/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    ContentsState,
    Queries,
    Query,
    ResourceOwner,
    SavedQuery,
    SchemasState,
    UIState
} from '@app/shared';

@Component({
    selector: 'sqx-contents-filters-page',
    styleUrls: ['./contents-filters-page.component.scss'],
    templateUrl: './contents-filters-page.component.html'
})
export class ContentsFiltersPageComponent extends ResourceOwner implements OnInit {
    public schemaQueries: Queries;

    constructor(
        public readonly contentsState: ContentsState,
        private readonly schemasState: SchemasState,
        private readonly uiState: UIState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    if (schema) {
                        this.schemaQueries = new Queries(this.uiState, `schemas.${schema.name}`);
                    }
                }));
    }

    public isQueryUsed = (query: SavedQuery) => {
        return this.contentsState.isQueryUsed(query);
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }

    public trackByQuery(index: number, query: { name: string }) {
        return query.name;
    }
}