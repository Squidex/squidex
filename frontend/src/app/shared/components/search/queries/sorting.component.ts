/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { QueryModel, QuerySorting, SORT_MODES } from '@app/shared/internal';
import { QueryPathComponent } from './query-path.component';

 @Component({
    selector: 'sqx-sorting',
    styleUrls: ['./sorting.component.scss'],
    templateUrl: './sorting.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        QueryPathComponent,
        FormsModule,
        NgFor,
    ],
})
export class SortingComponent {
    @Output()
    public sortingChange = new EventEmitter();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public model!: QueryModel;

    @Input({ required: true })
    public sorting!: QuerySorting;

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
        this.sortingChange.emit();
    }
}
