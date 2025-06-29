/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { SortOrder } from 'ngx-inline-filter';
import { TranslatePipe } from '@app/framework';
import { AppLanguageDto, Query, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-table-header',
    styleUrls: ['./table-header.component.scss'],
    templateUrl: './table-header.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class TableHeaderComponent {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public query: Query | undefined | null;

    @Input({ required: true })
    public text = '';

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public sortPath?: string | undefined | null;

    @Input({ transform: booleanAttribute })
    public sortable?: boolean | null;

    @Input()
    public sortDefault: SortOrder | undefined | null;

    public order: SortOrder | undefined | null;

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
