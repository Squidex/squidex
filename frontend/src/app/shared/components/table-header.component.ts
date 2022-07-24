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
    public text = '';

    @Input()
    public language!: LanguageDto;

    @Input()
    public sortPath?: string | undefined | null;

    @Input()
    public sortable?: boolean | null;

    @Input()
    public sortDefault: SortMode | undefined | null;

    public order: SortMode | undefined | null;

    public ngOnChanges() {
        const { query, sortDefault, sortable, sortPath } = this;

        if (!sortable) {
            this.order = null;
        } else if (sortPath && query?.sort?.length === 1 && query.sort[0].path === sortPath) {
            this.order = query.sort[0].order;
        } else if (sortDefault && !query?.sort?.length) {
            this.order = sortDefault;
        } else {
            this.order = null;
        }
    }

    public sort() {
        const { order, query, sortable, sortPath } = this;

        if (!sortable || !sortPath) {
            return;
        }

        if (!order || order !== 'ascending') {
            this.order = 'ascending';
        } else {
            this.order = 'descending';
        }

        const newQuery = Types.clone(query || {});

        newQuery.sort = [
            { path: sortPath, order: this.order! },
        ];

        this.queryChange.emit(newQuery);
    }
}
