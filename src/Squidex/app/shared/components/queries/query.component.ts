import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    Query,
    QueryModel,
    QuerySorting
} from '@app/shared/internal';

@Component({
    selector: 'sqx-query',
    template: `
        <div>
            <h4>Filter</h4>

            <sqx-filter-logical isRoot="true" [filter]="queryValue.filter" [model]="model"
                (change)="emitQueryChange()">
            </sqx-filter-logical>

            <h4 class="mt-4">Sorting</h4>

            <div class="mb-2" *ngFor="let sorting of queryValue.sort">
                <sqx-sorting [sorting]="sorting" [model]="model"
                    (remove)="removeSorting(sorting)" (change)="emitQueryChange()">
                </sqx-sorting>
            </div>

            <button class="btn btn-outline-success btn-sm mr-2" (click)="addSorting()">
                Add Sorting
            </button>
        </div>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class QueryComponent {
    @Output()
    public queryChange = new EventEmitter<Query>();

    @Input()
    public model: QueryModel;

    @Input()
    public set query(query: Query) {
        if (!query) {
            query = {};
        }

        if (query) {
            if (!query.filter) {
                query.filter = {
                    and: []
                };
            }

            if (!query.sort) {
                query.sort = [];
            }

            this.queryValue = query;
        }
    }

    public queryValue: Query;

    constructor() {
        this.query = {};
    }

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