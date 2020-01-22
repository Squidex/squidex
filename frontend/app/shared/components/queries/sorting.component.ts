/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { QueryModel, QuerySorting } from '@app/shared/internal';

 @Component({
     selector: 'sqx-sorting',
     template: `
        <div class="row">
            <div class="col">
                <div class="form-inline">
                    <sqx-query-path
                        (pathChange)="changePath($event)"
                        [path]="sorting.path"
                        [model]="model">
                    </sqx-query-path>

                    <select class="form-control ml-1" [ngModel]="sorting.order" (ngModelChange)="changeOrder($event)">
                        <option>ascending</option>
                        <option>descending</option>
                    </select>
                </div>
            </div>
            <div class="col-auto pl-2">
                <button type="button" class="btn btn-text-danger" (click)="remove.emit()">
                    <i class="icon-bin2"></i>
                </button>
            </div>
        </div>
    `,
     changeDetection: ChangeDetectionStrategy.OnPush
 })
export class SortingComponent {
    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public model: QueryModel;

    @Input()
    public sorting: QuerySorting;

    public changeOrder(order: any) {
        this.sorting.order = order;

        this.emitChange();
    }

    public changePath(path: string) {
        this.sorting.path = path;

        this.emitChange();
    }

    private emitChange() {
        this.change.emit();
    }
}