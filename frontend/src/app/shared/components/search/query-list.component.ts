/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { StopClickDirective, TranslatePipe } from '@app/framework';
import { equalsQuery, Query, SavedQuery } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-query-list',
    styleUrls: ['./query-list.component.scss'],
    templateUrl: './query-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NgFor,
        NgIf,
        StopClickDirective,
        TranslatePipe,
    ],
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

    @Input({ transform: booleanAttribute })
    public canRemove?: boolean | null;

    @Input()
    public types = '';

    public isSelectedQuery(saved: SavedQuery) {
        return equalsQuery(saved.query, this.queryUsed);
    }

    public trackByQuery(_index: number, query: SavedQuery) {
        return query.name;
    }
}
