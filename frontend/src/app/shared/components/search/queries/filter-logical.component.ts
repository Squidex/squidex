/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, numberAttribute, Output } from '@angular/core';
import { TranslatePipe } from '@app/framework';
import { FilterLogical, FilterNode, isLogicalAnd, isLogicalOr, LanguageDto, QueryModel } from '@app/shared/internal';
import { FilterNodeComponent } from './filter-node.component';

@Component({
    standalone: true,
    selector: 'sqx-filter-logical',
    styleUrls: ['./filter-logical.component.scss'],
    templateUrl: './filter-logical.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FilterNodeComponent,
        TranslatePipe,
    ],
})
export class FilterLogicalComponent {
    @Output()
    public filterChange = new EventEmitter<FilterLogical>();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public language!: LanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: numberAttribute })
    public level = 0;

    @Input({ transform: booleanAttribute })
    public isRoot?: boolean | null;

    @Input({ required: true })
    public model!: QueryModel;

    @Input({ required: true })
    public filter!: FilterLogical;

    public get filters() {
        return isLogicalAnd(this.filter) ? this.filter.and : this.filter.or;
    }

    public get isAnd() {
        return isLogicalAnd(this.filter);
    }

    public get isOr() {
        return isLogicalOr(this.filter);
    }

    public addComparison() {
        this.addNode({ path: this.model.schema.fields[0].path } as any);
    }

    public addLogical() {
        this.addNode({ and: [] });
    }

    public addNode(node: FilterNode) {
        const filter = this.filter;

        let change: FilterLogical;
        if (isLogicalAnd(filter)) {
            change = { and: [...filter.and, node] };
        } else {
            change = { or: [...filter.or, node] };
        }

        this.emitChange(change);
    }

    public removeNode(index: number) {
        const filter = this.filter;

        let change: FilterLogical;
        if (isLogicalAnd(filter)) {
            change = { and: filter.and.filter((_, i) => i !== index) };
        } else {
            change = { or: filter.or.filter((_, i) => i !== index) };
        }

        this.emitChange(change);
    }

    public replaceNode(index: number, node: FilterNode) {
        const filter = this.filter;

        let change: FilterLogical;
        if (isLogicalAnd(filter)) {
            change = { and: filter.and.map((x, i) => i === index ? node : x) };
        } else {
            change = { or: filter.or.map((x, i) => i === index ? node : x) };
        }

        this.emitChange(change);
    }

    public toggleType() {
        const filter = this.filter;

        let change: FilterLogical;
        if (isLogicalAnd(filter)) {
            change = { or: filter.and };
        } else {
            change = { and: filter.or };
        }

        this.emitChange(change);
    }

    private emitChange(filter: FilterLogical) {
        this.filterChange.emit(filter);
    }
}
