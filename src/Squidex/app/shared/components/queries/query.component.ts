import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    Query,
    QueryModel,
    QuerySorting
} from './model';

@Component({
    selector: 'sqx-query',
    template: `
        <div>
            <h4>Filter</h4>

            <sqx-filter-logical isRoot="true" [filter]="queryValue.filter" [model]="model"
                (change)="emitChange()">
            </sqx-filter-logical>

            <h4 class="mt-4">Sorting</h4>

            <div class="mb-2" *ngFor="let sorting of queryValue.sorting">
                <sqx-sorting [sorting]="sorting" [model]="model"
                    (remove)="removeSorting(sorting)" (change)="emitChange()">
                </sqx-sorting>
            </div>

            <button class="btn btn-outline-success btn-sm mr-2" (click)="addSorting()">
                Add Sorting
            </button>

            <h4 class="mt-4">Full Text</h4>

            <input class="form-control"
                [ngModel]="queryValue.fullText"
                (ngModelChange)="changeFullText($event)" />
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class QueryComponent {
    public queryValue: Query;

    public model: QueryModel = {
        fields: {
            'number': {
                type: 'number',
                operators: [
                    { value: 'eq', name: '==' },
                    { value: 'ne', name: '!=' },
                    { value: 'lt', name: '<' },
                    { value: 'le', name: '<=' },
                    { value: 'gt', name: '>' },
                    { value: 'ge', name: '>=' }
                ]
            },
            'string': {
                type: 'string',
                operators: [
                    { value: 'eq', name: '==' },
                    { value: 'ne', name: '!=' },
                    { value: 'startsWith' },
                    { value: 'endsWith' },
                    { value: 'contains' }
                ]
            },
            'boolean': {
                type: 'boolean',
                operators: [
                    { value: 'eq', name: '==' },
                    { value: 'ne', name: '!=' }
                ]
            }
        }
    };

    @Output()
    public change = new EventEmitter<Query>();

    @Input()
    public set query(query: Query) {
        if (query) {
            if (!query.filter) {
                query.filter = {
                    and: []
                };
            }

            if (!query.sorting) {
                query.sorting = [];
            }

            this.queryValue = query;
        }
    }

    constructor() {
        this.query = {};
    }

    public changeFullText(fullText: string) {
        this.query.fullText = fullText;

        this.emitChange();
    }

    public addSorting() {
        this.queryValue.sorting!.push({ path: Object.keys(this.model.fields)[0], order: 'ascending' });

        this.emitChange();
    }

    public removeSorting(sorting: QuerySorting) {
        this.queryValue.sorting!.splice(this.queryValue.sorting!.indexOf(sorting), 1);

        this.emitChange();
    }

    public emitChange() {
        this.change.emit();
    }
}