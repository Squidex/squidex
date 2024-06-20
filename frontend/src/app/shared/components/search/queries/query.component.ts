/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { FilterLogical, LanguageDto, Query, QueryModel, QuerySorting } from '@app/shared/internal';
import { FilterLogicalComponent } from './filter-logical.component';
import { SortingComponent } from './sorting.component';

@Component({
    standalone: true,
    selector: 'sqx-query',
    styleUrls: ['./query.component.scss'],
    templateUrl: './query.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FilterLogicalComponent,
        SortingComponent,
        TranslatePipe,
    ],
})
export class QueryComponent {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ required: true })
    public model!: QueryModel;

    @Input()
    public set query(query: Query | undefined | null) {
        if (!query) {
            query = {};
        }

        if (!query.filter) {
            query = { ...query, filter: { and: [] } };
        }

        if (!query.sort) {
            query = { ...query, sort: [] };
        }

        this.actualQuery = query as any;
    }

    public actualQuery!: RequireKeys<Query, 'sort' | 'filter'>;

    public addSorting() {
        const path = Object.keys(this.model.schema.fields)[0];

        this.change({ sort: [...this.actualQuery.sort, { path, order: 'ascending' }] });
    }

    public replaceSorting(index: number, sorting: QuerySorting) {
        this.change({ sort: this.actualQuery.sort.map((x, i) => i === index ? sorting : x) });
    }

    public removeSorting(index: number) {
        this.change({ sort: this.actualQuery.sort.filter((_, i) => i !== index) });
    }

    public changeFilter(filter: FilterLogical ) {
        this.change({ filter });
    }

    private change(update: Partial<Query>) {
        this.actualQuery = { ...this.actualQuery, ...update };

        this.queryChange.emit(this.actualQuery);
    }
}

type RequireKeys<T extends object, K extends keyof T> = Required<Pick<T, K>> & Omit<T, K>;