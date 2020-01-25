/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    equalsQuery,
    Query,
    SavedQuery
} from '@app/shared/internal';

@Component({
    selector: 'sqx-query-list',
    styleUrls: ['./query-list.component.scss'],
    templateUrl: './query-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class QueryListComponent {
    @Output()
    public search = new EventEmitter<Query>();

    @Output()
    public remove = new EventEmitter<SavedQuery>();

    @Input()
    public queryUsed: Query | undefined;

    @Input()
    public queries: ReadonlyArray<SavedQuery>;

    @Input()
    public canRemove: boolean;

    @Input()
    public types: string;

    public isSelectedQuery(saved: SavedQuery) {
        return equalsQuery(saved.query, this.queryUsed);
    }

    public trackByQuery(index: number, query: SavedQuery) {
        return query.name;
    }
}