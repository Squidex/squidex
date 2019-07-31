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
                    <select class="form-control mr-2" [ngModel]="filter.path" (ngModelChange)="changePath($event)">
                        <option *ngFor="let fieldName of model.fields | sqxKeys" [ngValue]="fieldName">{{fieldName}}</option>
                    </select>

                    <ng-container *ngIf="fieldModel">
                        <select class="form-control mr-2" [ngModel]="filter.op" (ngModelChange)="changeOp($event)">
                            <option *ngFor="let operator of fieldModel.operators" [ngValue]="operator.value">{{operator.name || operator.value}}</option>
                        </select>

                        <ng-container [ngSwitch]="fieldModel.type">
                            <ng-container *ngSwitchCase="'boolean'">
                                <input class="form-control"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)" type="checkbox" />
                            </ng-container>
                            <ng-container *ngSwitchCase="'date'">
                                <sqx-date-time-editor
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)"></sqx-date-time-editor>
                            </ng-container>
                            <ng-container *ngSwitchCase="'datetime'">
                                <sqx-date-time-editor
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)"></sqx-date-time-editor>
                            </ng-container>
                            <ng-container *ngSwitchCase="'number'">
                                <input class="form-control"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)" type="number" />
                            </ng-container>
                            <ng-container *ngSwitchCase="'string'">
                                <input class="form-control"
                                    [ngModel]="filter.value"
                                    (ngModelChange)="changeValue($event)" type="text" />
                            </ng-container>
                        </ng-container>
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
         'row: padding: .5rem;'
     ],
     changeDetection: ChangeDetectionStrategy.OnPush
 })
export class FilterComparisonComponent implements OnChanges {
    public fieldModel: QueryFieldModel;

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
        const newModel = this.model.fields[this.filter.path];

        if (newModel && refresh) {
            if (!newModel.operators.find(x => x.value === this.filter.op)) {
                this.filter.op = newModel.operators[0].value;
            }

            if (!this.fieldModel || this.fieldModel.type !== newModel.type) {
                this.filter.value = undefined;
            }
        }

        this.fieldModel = newModel;
    }

    private emitChange() {
        this.change.emit();
    }
}