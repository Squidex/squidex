/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { Sorting } from '@app/shared/internal';

@Component({
    selector: 'sqx-table-header',
    template: `
        <a *ngIf="sortable; else notSortable" (click)="sort()" class="pointer truncate">
            <i *ngIf="sorting === 'Ascending'" class="icon-caret-down"></i>
            <i *ngIf="sorting === 'Descending'" class="icon-caret-up"></i>

            {{text}}
        </a>

        <ng-template #notSortable>
            {{text}}
        </ng-template>`,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TableHeaderComponent {
    @Input()
    public text: string;

    @Input()
    public sortable = false;

    @Input()
    public sorting: Sorting;

    @Output()
    public sortingChange = new EventEmitter<Sorting>();

    public sort() {
        if (this.sortable) {
            if (!this.sorting || this.sorting !== 'Ascending') {
                this.sorting = 'Ascending';
            } else {
                this.sorting = 'Descending';
            }

            this.sortingChange.emit(this.sorting);
        }
    }
}