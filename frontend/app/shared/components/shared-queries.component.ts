/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { Queries, Query } from '@app/shared/internal';

@Component({
    selector: 'sqx-shared-queries',
    template: `
        <div class="sidebar-section">
            <h3>Shared queries</h3>

            <sqx-query-list
                [canRemove]="true"
                [queryUsed]="queryUsed"
                [queries]="queries.queriesShared | async"
                (search)="search.emit($event)"
                (remove)="queries.removeShared($event)">
            </sqx-query-list>
        </div>

        <hr />

        <div class="sidebar-section">
            <h3>My queries</h3>

            <sqx-query-list
                [canRemove]="true"
                [queryUsed]="queryUsed"
                [queries]="queries.queriesUser | async"
                (search)="search.emit($event)"
                (remove)="queries.removeUser($event)">
            </sqx-query-list>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SavedQueriesComponent {
    @Output()
    public search = new EventEmitter<Query>();

    @Input()
    public queryUsed: Query;

    @Input()
    public queries: Queries;

    @Input()
    public types: string;
}