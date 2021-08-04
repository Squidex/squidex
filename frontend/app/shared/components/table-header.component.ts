/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { LanguageDto, Query, SortMode, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-table-header[text]',
    styleUrls: ['./table-header.component.scss'],
    templateUrl: './table-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableHeaderComponent implements OnChanges {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query | undefined | null;

    @Input()
    public text: string;

    @Input()
    public fieldPath?: string | undefined | null;

    @Input()
    public language: LanguageDto;

    @Input()
    public sortable?: boolean | null;

    @Input()
    public defaultOrder: SortMode | undefined | null;

    public order: SortMode | undefined | null;

    public ngOnChanges() {
        if (this.sortable) {
            const { sort } = this.query || {};

            if (this.fieldPath && sort && sort.length === 1 && sort[0].path === this.fieldPath) {
                this.order = sort[0].order;
            } else if (this.defaultOrder && (!sort || sort.length === 0)) {
                this.order = this.defaultOrder;
            } else {
                this.order = null;
            }
        } else {
            this.order = null;
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
