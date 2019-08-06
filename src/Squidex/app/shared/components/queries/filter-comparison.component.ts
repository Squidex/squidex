/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    fadeAnimation,
    FilterComparison,
    QueryFieldModel,
    QueryModel
} from '@app/shared/internal';
import { StatusInfo } from '@app/shared/services/contents.service';

 @Component({
    selector: 'sqx-filter-comparison',
    styleUrls: ['./filter-comparison.component.scss'],
    templateUrl: './filter-comparison.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterComparisonComponent implements OnChanges {
    public fieldModel: QueryFieldModel;

    public noValue = false;

    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public model: QueryModel;

    @Input()
    public filter: FilterComparison;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['filter']) {
            this.updatePath(false);
            this.updateOperator();
        }
    }

    public getStatus(statuses: StatusInfo[]) {
        return statuses.find(x => x.status === this.filter.value);
    }

    public changeStatus(status: StatusInfo) {
        this.changeValue(status.status);
    }

    public changeValue(value: any) {
        this.filter.value = value;

        this.emitChange();
    }

    public changeOp(op: string) {
        this.filter.op = op;

        this.updateOperator();

        this.emitChange();
    }

    public changePath(path: string) {
        this.filter.path = path;

        this.updatePath(true);

        this.emitChange();
    }

    private updateOperator() {
        if (this.fieldModel) {
            const operator = this.fieldModel.operators.find(x => x.value === this.filter.op);

            this.noValue = !!(operator && operator.noValue);
        }
    }

    private updatePath(refresh: boolean) {
        const newModel = this.model.fields[this.filter.path];

        if (newModel && refresh) {
            if (!newModel.operators.find(x => x.value === this.filter.op)) {
                this.filter.op = newModel.operators[0].value;
            }

            this.filter.value = null;
        }

        this.fieldModel = newModel;
    }

    private emitChange() {
        this.change.emit();
    }
}