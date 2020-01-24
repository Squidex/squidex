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
    template: `
        <ng-container *ngIf="queries.length > 0; else noQuery">
            <a class="sidebar-item" *ngFor="let saved of queries; trackBy: trackByQuery" (click)="search.emit(saved.query)"
                [class.active]="isSelectedQuery(saved)">

                <i class="icon-circle" [style.color]="saved.color" [class.hidden]="!saved.color"></i> {{saved.name}}

                <a class="sidebar-item-remove float-right" (click)="remove.emit(saved)" *ngIf="canRemove" sqxStopClick>
                    <i class="icon-close"></i>
                </a>
            </a>
        </ng-container>

        <ng-template #noQuery>
            <div class="sidebar-item inactive text-muted" *ngIf="canRemove">
                Search for {{types}} and use <i class="icon-star-empty"></i> icon in search form to save query for all contributors.
            </div>
        </ng-template>
    `,
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