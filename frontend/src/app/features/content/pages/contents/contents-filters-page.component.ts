/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ContentsState, LayoutComponent, Queries, Query, QueryListComponent, SavedQueriesComponent, SchemasState, TranslatePipe, UIState } from '@app/shared';

@Component({
    selector: 'sqx-contents-filters-page',
    styleUrls: ['./contents-filters-page.component.scss'],
    templateUrl: './contents-filters-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LayoutComponent,
        QueryListComponent,
        SavedQueriesComponent,
        TranslatePipe,
    ],
})
export class ContentsFiltersPageComponent {
    private readonly schemasState = inject(SchemasState);
    private readonly uiState = inject(UIState);

    protected readonly contentsState = inject(ContentsState);

    protected readonly schemaQueries = computed(() => {
        const schema = this.schemasState.selectedSchema();
        if (!schema) {
            return null;
        }

        return new Queries(this.uiState, `schemas.${schema.name}`);
    });

    protected search(query?: Query) {
        this.contentsState.search(query);
    }
}
