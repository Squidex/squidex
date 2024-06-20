/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { QueryModel, QuerySorting, SORT_MODES } from '@app/shared/internal';
import { QueryPathComponent } from './query-path.component';

@Component({
    standalone: true,
    selector: 'sqx-sorting',
    styleUrls: ['./sorting.component.scss'],
    templateUrl: './sorting.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        QueryPathComponent,
    ],
})
export class SortingComponent {
    public readonly modes = SORT_MODES;

    @Output()
    public sortingChange = new EventEmitter<QuerySorting>();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public model!: QueryModel;

    @Input({ required: true })
    public sorting!: QuerySorting;

    public changeOrder(order: any) {
        this.change({ order });
    }

    public changePath(path: string) {
        this.change({ path });
    }

    private change(update: Partial<QuerySorting>) {
        this.sorting = { ...this.sorting, ...update };

        this.sortingChange.emit(this.sorting);
    }
}
