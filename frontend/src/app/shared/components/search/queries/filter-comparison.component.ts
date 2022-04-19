/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FilterableField, FilterComparison, FilterFieldUI, getFilterUI, LanguageDto, QueryModel, StatusInfo } from '@app/shared/internal';
import { ContributorsState } from '@app/shared/state/contributors.state';

@Component({
    selector: 'sqx-filter-comparison[filter][language][languages][model][statuses]',
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
    public statuses?: ReadonlyArray<StatusInfo> | null;

    @Input()
    public model!: QueryModel;

    @Input()
    public filter!: FilterComparison;

    public field?: FilterableField;
    public fieldUI?: FilterFieldUI;
    public operators: ReadonlyArray<string> = [];

    constructor(
        public readonly contributorsState: ContributorsState,
    ) {
    }

    public ngOnChanges() {
        this.updatePath(false);
    }

    public changeValue(value: any) {
        this.filter.value = value;

        this.emitChange();
    }

    public changeOp(op: string) {
        this.filter.op = op;

        this.updatePath(false);

        this.emitChange();
    }

    public changePath(path: string) {
        this.filter.path = path;

        this.updatePath(true);

        this.emitChange();
    }

    private updatePath(updateValue: boolean) {
        this.field = this.model.schema.fields.find(x => x.path === this.filter.path);

        this.operators = this.model.operators[this.field?.schema.type!] || [];

        if (!this.operators.includes(this.filter.op)) {
            this.filter.op = this.operators[0];
        }

        if (updateValue) {
            this.filter.value = null;
        }

        this.fieldUI = getFilterUI(this.filter, this.field!);
    }

    public emitChange() {
        this.change.emit();
    }
}
