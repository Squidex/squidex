/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    FilterComparison,
    FilterLogical,
    FilterNode,
    LanguageDto,
    QueryModel
} from '@app/shared/internal';

@Component({
    selector: 'sqx-filter-node',
    template: `
        <ng-container *ngIf="logical">
            <sqx-filter-logical [model]="model" [filter]="logical" [language]="language" [level]="level"
                (remove)="remove.emit()" (change)="change.emit()">
            </sqx-filter-logical>
        </ng-container>

        <ng-container *ngIf="comparison">
            <sqx-filter-comparison [model]="model" [filter]="comparison" [language]="language"
                (remove)="remove.emit()" (change)="change.emit()">
            </sqx-filter-comparison>
        </ng-container>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterNodeComponent {
    public comparison: FilterComparison;

    @Output()
    public change = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public language: LanguageDto;

    @Input()
    public level: number;

    @Input()
    public model: QueryModel;

    @Input()
    public set filter(value: FilterNode) {
        if (value['and'] || value['or']) {
            this.logical = <FilterLogical>value;
        } else {
            this.comparison = <FilterComparison>value;
        }
    }

    public logical: FilterLogical;
}