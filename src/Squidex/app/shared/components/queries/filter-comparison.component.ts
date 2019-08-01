/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

import {
    FilterComparison,
    QueryFieldModel,
    QueryModel
} from './model';

 @Component({
     selector: 'sqx-filter-comparison',
     template: `
        <div class="row">
            <div class="col">
                <div class="form-inline">
                    <select class="form-control path mb-1 mr-2" [ngModel]="filter.path" (ngModelChange)="changePath($event)">
                        <option *ngFor="let fieldName of model.fields | sqxKeys" [ngValue]="fieldName">{{fieldName}}</option>
                    </select>

                    <ng-container *ngIf="fieldModel">
                        <select class="form-control mb-1 mr-2" [ngModel]="filter.op" (ngModelChange)="changeOp($event)">
                            <option *ngFor="let operator of fieldModel.operators" [ngValue]="operator.value">{{operator.name || operator.value}}</option>
                        </select>

                        <div class="mb-1" *ngIf="!noValue" [ngSwitch]="fieldModel.type">
                            <ng-container *ngSwitchCase="'boolean'">
                                <input type="checkbox" class="form-control"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)" />
                            </ng-container>
                            <ng-container *ngSwitchCase="'date'">
                                <sqx-date-time-editor mode="Date"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)">
                                </sqx-date-time-editor>
                            </ng-container>
                            <ng-container *ngSwitchCase="'datetime'">
                                <sqx-date-time-editor mode="DateTime"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)">
                                </sqx-date-time-editor>
                            </ng-container>
                            <ng-container *ngSwitchCase="'number'">
                                <input type="number" class="form-control"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)" />
                            </ng-container>
                            <ng-container *ngSwitchCase="'string'">
                                <ng-container *ngIf="fieldModel.extra?.values; let values">
                                    <select class="form-control"
                                        [ngModel]="filter.value"
                                        (ngModelChange)="changeValue($event)">
                                        <option *ngFor="let value of values" [ngValue]="value">{{value}}</option>
                                    </select>
                                </ng-container>

                                <ng-container *ngIf="fieldModel.extra?.schemaId; let schemaId">
                                    <sqx-references-dropdown [schemaId]="schemaId"
                                        mode="SingleValue"
                                        [ngModel]="filter.value"
                                        (ngModelChange)="changeValue($event)"
                                        [isRequired]="true">
                                    </sqx-references-dropdown>
                                </ng-container>

                                <input type="text" class="form-control" *ngIf="!fieldModel.extra"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)" />
                            </ng-container>
                        </div>
                    </ng-container>
                </div>
            </div>
            <div class="col-auto pl-2">
                <button type="button" class="btn btn-text-danger" (click)="remove.emit()">
                    <i class="icon-bin2"></i>
                </button>
            </div>
        </div>
    `,
    styles: [
        '.path { max-width: 12rem; }'
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