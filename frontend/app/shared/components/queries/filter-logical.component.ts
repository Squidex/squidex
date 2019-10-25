/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

 // tslint:disable: readonly-array

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    fadeAnimation,
    FilterLogical,
    FilterNode,
    LanguageDto,
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

    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public language: LanguageDto;

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

    public get isAnd() {
        return !!this.filter.and;
    }

    public get isOr() {
        return !!this.filter.or;
    }

    public get nestedLevel() {
        return this.level + 1;
    }

    public filters: FilterNode[] = [];

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

    public emitRemove() {
        this.remove.emit();
    }

    public emitChange() {
        this.change.emit();
    }
}