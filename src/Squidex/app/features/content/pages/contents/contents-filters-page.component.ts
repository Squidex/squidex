/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    ContentsState,
    Queries,
    ResourceOwner,
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
        private readonly contentsState: ContentsState,
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

    public search(query: string) {
        this.contentsState.search(query).pipe(onErrorResumeNext()).subscribe();
    }

    public isSelectedQuery(query: string) {
        return query === this.contentsState.snapshot.contentsQuery || (!query && !this.contentsState.snapshot.contentsQuery);
    }

    public trackByTag(index: number, tag: { name: string }) {
        return tag.name;
    }

    public trackByQuery(index: number, query: { name: string }) {
        return query.name;
    }
}