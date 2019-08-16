/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    fadeAnimation,
    FilterLogical,
    FilterNode,
    QueryModel
} from '@app/shared/internal';

@Component({
    selector: 'sqx-filter-logical',
    styleUrls: ['./filter-logical.component.scss'],
    templateUrl: './filter-logical.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterLogicalComponent {
    private filterValue: FilterLogical;

    public filters: FilterNode[] = [];

    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public level = 0;

    @Input()
    public isRoot = false;

    @Input()
    public model: QueryModel;

    @Input()
    public set filter(filter: FilterLogical) {
        this.filterValue = filter;

        this.updateFilters(filter);
    }

    public get filter() {
        return this.filterValue;
    }

    public addComparison() {
        this.filters.push(<any>{ path: Object.keys(this.model.fields)[0] });

        this.emitChange();
    }

    public addLogical() {
        this.filters.push({ and: [] });

        this.emitChange();
    }

    public removeFilter(node: FilterNode) {
        this.filters.splice(this.filters.indexOf(node), 1);

        this.emitChange();
    }

    public toggleType() {
        if (this.filterValue.and) {
            this.filterValue.or = this.filterValue.and;

            delete this.filterValue.and;
        } else {
            this.filterValue.and = this.filterValue.or;

            delete this.filterValue.or;
        }

        this.emitChange();
    }

    private updateFilters(filter: FilterLogical) {
        if (filter) {
            this.filters = filter.and || filter.or || [];
        } else {
            this.filters = [];
        }
    }

    private emitChange() {
        this.change.emit();
    }
}