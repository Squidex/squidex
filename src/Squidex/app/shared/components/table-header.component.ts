/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { SortMode } from '@app/shared/internal';

@Component({
    selector: 'sqx-table-header',
    template: `
        <a *ngIf="sortable; else notSortable" (click)="sort()" class="pointer truncate">
            <i *ngIf="order === 'ascending'" class="icon-caret-down"></i>
            <i *ngIf="order === 'descending'" class="icon-caret-up"></i>

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
    public order: SortMode;

    @Output()
    public orderChange = new EventEmitter<SortMode>();

    public sort() {
        if (this.sortable) {
            if (!this.order || this.order !== 'ascending') {
                this.order = 'ascending';
            } else {
                this.order = 'descending';
            }

            this.orderChange.emit(this.order);
        }
    }
}