/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PagingInfo } from '../state';
import { TooltipDirective } from './modals/tooltip.directive';
import { TranslatePipe } from './pipes/translate.pipe';

export const PAGE_SIZES: ReadonlyArray<number> = [10, 20, 30, 50];

@Component({
    standalone: true,
    selector: 'sqx-pager',
    styleUrls: ['./pager.component.scss'],
    templateUrl: './pager.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        NgFor,
        NgIf,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class PagerComponent {
    @Output()
    public loadTotal = new EventEmitter();

    @Output()
    public pagingChange = new EventEmitter<{ page: number; pageSize: number }>();

    @Input({ required: true })
    public paging: PagingInfo | undefined | null;

    @Input({ transform: booleanAttribute })
    public autoHide?: boolean | null;

    public totalPages = 0;
    public totalItems = 0;

    public itemFirst = 0;
    public itemLast = 0;

    public canGoPrev?: boolean | null;
    public canGoNext = false;

    public translationInfo: any;

    public pageSizes = PAGE_SIZES;

    public ngOnChanges() {
        if (!this.paging) {
            return;
        }

        const { page, pageSize, count, total } = this.paging;

        const offset = page * pageSize;

        this.itemFirst = offset + (count > 0 ? 1 : 0);
        this.itemLast = offset + count;

        if (count > 0 && total >= 0) {
            const totalPages = Math.ceil(total / pageSize);

            this.canGoNext = page < totalPages - 1;
        } else if (count > 0) {
            this.canGoNext = count === pageSize;
        } else {
            this.canGoNext = false;
        }

        this.canGoPrev = page > 0;

        this.translationInfo = {
            itemFirst: this.itemFirst,
            itemLast: this.itemLast,
            numberOfItems: total,
        };
    }

    public goPrev() {
        if (!this.paging) {
            return;
        }

        const { page, pageSize } = this.paging;

        this.pagingChange.emit({ page: page - 1, pageSize });
    }

    public goNext() {
        if (!this.paging) {
            return;
        }

        const { page, pageSize } = this.paging;

        this.pagingChange.emit({ page: page + 1, pageSize });
    }

    public setPageSize(pageSize: number) {
        this.pagingChange.emit({ page: 0, pageSize });
    }
}
