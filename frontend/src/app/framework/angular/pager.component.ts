/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { PagingInfo } from '../state';

export const PAGE_SIZES: ReadonlyArray<number> = [10, 20, 30, 50];

@Component({
    selector: 'sqx-pager[paging]',
    styleUrls: ['./pager.component.scss'],
    templateUrl: './pager.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PagerComponent implements OnChanges {
    @Output()
    public pagingChange = new EventEmitter<{ page: number; pageSize: number }>();

    @Input()
    public paging: PagingInfo | undefined | null;

    @Input()
    public autoHide?: boolean | null;

    public totalPages = 0;

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

        const totalPages = Math.ceil(total / pageSize);

        if (count > 0) {
            const offset = page * pageSize;

            this.itemFirst = offset + 1;
            this.itemLast = offset + count;

            this.canGoNext = page < totalPages - 1;
            this.canGoPrev = page > 0;
        } else {
            this.canGoNext = false;
            this.canGoPrev = false;
        }

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
