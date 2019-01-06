/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter, onErrorResumeNext, takeUntil } from 'rxjs/operators';

import {
    ContentsState,
    navigatedToOtherComponent,
    Queries,
    SchemasState,
    UIState
} from '@app/shared';

@Component({
    selector: 'sqx-contents-filters-page',
    styleUrls: ['./contents-filters-page.component.scss'],
    templateUrl: './contents-filters-page.component.html'
})
export class ContentsFiltersPageComponent implements OnDestroy, OnInit {
    private selectedSchemaSubscription: Subscription;

    public schemaQueries: Queries;

    constructor(
        private readonly contentsState: ContentsState,
        private readonly schemasState: SchemasState,
        private readonly router: Router,
        private readonly uiState: UIState
    ) {
    }

    public ngOnDestroy() {
        this.selectedSchemaSubscription.unsubscribe();
    }

    public ngOnInit() {
        const routeChanged = this.router.events.pipe(filter(navigatedToOtherComponent(this.router)));

        this.selectedSchemaSubscription =
            this.schemasState.selectedSchema.pipe(takeUntil(routeChanged))
                .subscribe(schema => {
                    if (schema) {
                        this.schemaQueries = new Queries(this.uiState, `schemas.${schema.name}`);
                    }
                });
    }

    public search(query: string) {
        this.contentsState.search(query).pipe(onErrorResumeNext()).subscribe();
    }

    public isSelectedQuery(query: string) {
        return query === this.contentsState.snapshot.contentsQuery || (!query && !this.contentsState.snapshot.contentsQuery);
    }
}