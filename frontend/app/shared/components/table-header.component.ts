/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    LanguageDto,
    Query,
    SortMode
} from '@app/shared/internal';

@Component({
    selector: 'sqx-table-header',
    template: `
        <a *ngIf="sortable; else notSortable" (click)="sort()" class="pointer truncate">
            <span class="truncate">
                <i *ngIf="order === 'ascending'" class="icon-caret-down"></i>
                <i *ngIf="order === 'descending'" class="icon-caret-up"></i>

                {{text}}
            </span>
        </a>

        <ng-template #notSortable>
            <span class="truncate">{{text}}</span>
        </ng-template>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TableHeaderComponent implements OnChanges {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query;

    @Input()
    public text: string;

    @Input()
    public fieldPath: string;

    @Input()
    public language: LanguageDto;

    @Input()
    public sortable = false;

    public order: SortMode | null;

    public ngOnChanges(changes: SimpleChanges) {
        if (this.sortable) {
            if (changes['query'] || changes['fieldPath']) {
                if (this.fieldPath &&
                    this.query &&
                    this.query.sort &&
                    this.query.sort.length === 1 &&
                    this.query.sort[0].path === this.fieldPath) {
                    this.order = this.query.sort[0].order;
                } else {
                    this.order = null;
                }
            }
        }
    }

    public sort() {
        if (this.sortable && this.fieldPath) {
            if (!this.order || this.order !== 'ascending') {
                this.order = 'ascending';
            } else {
                this.order = 'descending';
            }

            this.queryChange.emit(this.newQuery());
        }
    }

    private newQuery() {
        return {...this.query, sort: [{ path: this.fieldPath, order: this.order! }] };
    }
}