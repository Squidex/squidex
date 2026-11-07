/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, input, linkedSignal, output } from '@angular/core';
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
    /** Emits the new query with the new sort order when the header is clicked. */
    public readonly queryChange = output<Query>();

    /** The current query, to show the sort order. */
    public readonly query = input<Query | null>();

    /** The text of the header. */
    public readonly text = input.required<string>();

    /** The language for localized fields. */
    public readonly language = input<AppLanguageDto>();

    /** The path to sort by, for example "data.name.iv". */
    public readonly sortPath = input<string | null>();

    /** Allows sorting by clicking the header. */
    public readonly sortable = input(false, { transform: booleanAttribute });

    /** The sort order to show when the query has no sorting. */
    public readonly sortDefault = input<SortOrder | null>();

    protected readonly order = linkedSignal<SortOrder | null>(() => {
        if (!this.sortable()) {
            return null;
        }

        const query = this.query();
        const sortPath = this.sortPath();
        if (sortPath && query?.sort?.length === 1 && query.sort[0].path === sortPath) {
            return query.sort[0].order;
        }

        const sortDefault = this.sortDefault();
        if (sortDefault && !query?.sort?.length) {
            return sortDefault;
        }

        return null;
    });

    protected sort() {
        const sortPath = this.sortPath();
        if (!this.sortable() || !sortPath) {
            return;
        }

        const order = this.order() === 'ascending' ? 'descending' : 'ascending';

        this.order.set(order);

        const query = Types.clone(this.query() || {});
        query.sort = [
            { path: sortPath, order },
        ];

        this.queryChange.emit(query);
    }
}
