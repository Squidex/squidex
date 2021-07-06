/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { equalsQuery, Query, SavedQuery } from '@app/shared/internal';

@Component({
    selector: 'sqx-query-list',
    styleUrls: ['./query-list.component.scss'],
    templateUrl: './query-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QueryListComponent {
    @Output()
    public search = new EventEmitter<Query>();

    @Output()
    public remove = new EventEmitter<SavedQuery>();

    @Input()
    public queryUsed?: Query | null;

    @Input()
    public queries?: ReadonlyArray<SavedQuery> | null;

    @Input()
    public canRemove?: boolean | null;

    @Input()
    public types: string;

    public isSelectedQuery(saved: SavedQuery) {
        return equalsQuery(saved.query, this.queryUsed);
    }

    public trackByQuery(_index: number, query: SavedQuery) {
        return query.name;
    }
}
