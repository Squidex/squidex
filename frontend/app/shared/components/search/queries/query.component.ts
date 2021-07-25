/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { LanguageDto, Query, QueryModel } from '@app/shared/internal';

@Component({
    selector: 'sqx-query',
    styleUrls: ['./query.component.scss'],
    templateUrl: './query.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QueryComponent {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public language: LanguageDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public model: QueryModel;

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
        const path = Object.keys(this.model.fields)[0];

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
