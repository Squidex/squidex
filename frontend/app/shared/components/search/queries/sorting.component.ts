/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { QueryModel, QuerySorting, SORT_MODES } from '@app/shared/internal';

 @Component({
     selector: 'sqx-sorting',
     styleUrls: ['./sorting.component.scss'],
     templateUrl: './sorting.component.html',
     changeDetection: ChangeDetectionStrategy.OnPush,
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

    public modes = SORT_MODES;

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
