/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FilterLogical, FilterNode, LanguageDto, QueryModel, StatusInfo } from '@app/shared/internal';

@Component({
    selector: 'sqx-filter-logical[filter][language][languages][model][statuses]',
    styleUrls: ['./filter-logical.component.scss'],
    templateUrl: './filter-logical.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterLogicalComponent {
    private filterValue!: FilterLogical;

    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public statuses?: ReadonlyArray<StatusInfo> | null;

    @Input()
    public level = 0;

    @Input()
    public isRoot?: boolean | null;

    @Input()
    public model!: QueryModel;

    @Input()
    public set filter(filter: FilterLogical | undefined | null) {
        this.filterValue = filter || {};

        this.updateFilters(this.filterValue);
    }

    public filters: FilterNode[] = [];

    public get filter() {
        return this.filterValue;
    }

    public get isAnd() {
        return !!this.filterValue.and;
    }

    public get isOr() {
        return !!this.filterValue.or;
    }

    public addComparison() {
        this.filters.push(<any>{ path: this.model.schema.fields[0].path });

        this.emitChange();
    }

    public addLogical() {
        this.filters.push({ and: [] });

        this.emitChange();
    }

    public removeFilter(index: number) {
        this.filters.splice(index, 1);

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

    public emitChange() {
        this.change.emit();
    }
}
