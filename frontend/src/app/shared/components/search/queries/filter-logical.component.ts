/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, numberAttribute, Output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { FilterLogical, FilterNode, LanguageDto, QueryModel, StatusInfo } from '@app/shared/internal';
import { FilterNodeComponent } from './filter-node.component';

@Component({
    standalone: true,
    selector: 'sqx-filter-logical',
    styleUrls: ['./filter-logical.component.scss'],
    templateUrl: './filter-logical.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FilterNodeComponent,
        NgFor,
        NgIf,
        TranslatePipe,
    ],
})
export class FilterLogicalComponent {
    private filterValue!: FilterLogical;

    @Output()
    public filterChange = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ required: true })
    public statuses?: ReadonlyArray<StatusInfo> | null;

    @Input({ transform: numberAttribute })
    public level = 0;

    @Input({ transform: booleanAttribute })
    public isRoot?: boolean | null;

    @Input({ required: true })
    public model!: QueryModel;

    @Input({ required: true })
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
        this.filterChange.emit();
    }
}
