/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { LanguageDto, Query, SortMode, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-table-header',
    styleUrls: ['./table-header.component.scss'],
    templateUrl: './table-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableHeaderComponent implements OnChanges {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query | undefined;

    @Input()
    public text: string;

    @Input()
    public fieldPath: string;

    @Input()
    public language: LanguageDto;

    @Input()
    public sortable?: boolean | null;

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

            const newQuery = Types.clone(this.query || {});

            newQuery.sort = [
                { path: this.fieldPath, order: this.order! },
            ];

            this.queryChange.emit(newQuery);
        }
    }
}
