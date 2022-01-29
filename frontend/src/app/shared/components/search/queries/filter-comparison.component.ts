/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FilterComparison, LanguageDto, FilterableField, QueryModel } from '@app/shared/internal';
import { ContributorsState } from '@app/shared/state/contributors.state';

@Component({
    selector: 'sqx-filter-comparison[filter][language][languages][model]',
    styleUrls: ['./filter-comparison.component.scss'],
    templateUrl: './filter-comparison.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterComparisonComponent implements OnChanges {
    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public language!: LanguageDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public model!: QueryModel;

    @Input()
    public filter!: FilterComparison;

    public field?: FilterableField;

    public get operators() {
        return this.model.operators[this.field?.schema.type!] || [];
    }

    public get noValue() {
        return this.filter.op === 'empty' || this.filter.op === 'exists';
    }

    constructor(
        public readonly contributorsState: ContributorsState,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['filter']) {
            this.updatePath(false);
        }
    }

    public changeValue(value: any) {
        this.filter.value = value;

        this.emitChange();
    }

    public changeOp(op: string) {
        this.filter.op = op;

        this.emitChange();
    }

    public changePath(path: string) {
        this.filter.path = path;

        this.updatePath(true);

        this.emitChange();
    }

    private updatePath(refresh: boolean) {
        const newModel = this.model.schema.fields.find(x => x.path === this.filter.path);

        if (newModel && refresh) {
            const operators = this.model.operators[newModel.schema.type];

            if (operators && operators.indexOf(this.filter.op) < 0) {
                this.filter.op = operators[0];
            }

            this.filter.value = null;
        }

        this.field = newModel;
    }

    public emitChange() {
        this.change.emit();
    }
}
