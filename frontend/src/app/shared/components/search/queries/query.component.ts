/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { LanguageDto, Query, QueryModel, StatusInfo } from '@app/shared/internal';
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
        NgFor,
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
    public statuses?: ReadonlyArray<StatusInfo> | null;

    @Input({ required: true })
    public model!: QueryModel;

    @Input()
    public set query(query: Query | undefined | null) {
        if (!query) {
            query = {};
        }

        if (!query.filter) {
            query.filter = { and: [] };
        }

        if (!query.sort) {
            query.sort = [];
        }

        this.queryValue = query;
    }

    public queryValue: Query = {};

    public addSorting() {
        const path = Object.keys(this.model.schema.fields)[0];

        if (this.queryValue.sort) {
            this.queryValue.sort.push({ path, order: 'ascending' });
        }

        this.emitQueryChange();
    }

    public removeSorting(index: number) {
        if (this.queryValue.sort) {
            this.queryValue.sort.splice(index, 1);
        }

        this.emitQueryChange();
    }

    public emitQueryChange() {
        this.queryChange.emit(this.queryValue);
    }
}
