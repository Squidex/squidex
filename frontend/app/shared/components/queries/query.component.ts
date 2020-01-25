/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    LanguageDto,
    Query,
    QueryModel,
    QuerySorting,
    Types
} from '@app/shared/internal';

@Component({
    selector: 'sqx-query',
    styleUrls: ['./query.component.scss'],
    templateUrl: './query.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class QueryComponent {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public language: LanguageDto;

    @Input()
    public model: QueryModel;

    @Input()
    public set query(query: Query) {
        this.queryValue = Types.clone(query);
    }

    public queryValue: Query = {};

    public addSorting() {
        this.queryValue.sort!.push({ path: Object.keys(this.model.fields)[0], order: 'ascending' });

        this.emitQueryChange();
    }

    public removeSorting(sorting: QuerySorting) {
        this.queryValue.sort!.splice(this.queryValue.sort!.indexOf(sorting), 1);

        this.emitQueryChange();
    }

    public emitQueryChange() {
        this.queryChange.emit(this.queryValue);
    }
}