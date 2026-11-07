/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PagingInfo } from '../state';
import { TooltipDirective } from './modals/tooltip.directive';
import { TranslatePipe } from './pipes/translate.pipe';

export const PAGE_SIZES: ReadonlyArray<number> = [10, 20, 30, 50];

@Component({
    selector: 'sqx-pager',
    styleUrls: ['./pager.component.scss'],
    templateUrl: './pager.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class PagerComponent {
    /** Emits when the total number of items should be loaded, because it is not known yet. */
    public readonly loadTotal = output();

    /** Emits the new page and page size when the user navigates or changes the page size. */
    public readonly pagingChange = output<{ page: number; pageSize: number }>();

    /** The current paging state. */
    public readonly paging = input.required<PagingInfo | undefined | null>();

    /** Hides the pager when there is only one page. */
    public readonly autoHide = input(false, { transform: booleanAttribute });

    public readonly pageSizes = PAGE_SIZES;

    public readonly itemFirst = computed(() => {
        const paging = this.paging();
        if (!paging) {
            return 0;
        }

        return paging.page * paging.pageSize + (paging.count > 0 ? 1 : 0);
    });

    public readonly itemLast = computed(() => {
        const paging = this.paging();
        if (!paging) {
            return 0;
        }

        return paging.page * paging.pageSize + paging.count;
    });

    public readonly canGoPrev = computed(() => {
        const paging = this.paging();
        if (!paging) {
            return false;
        }

        return paging.page > 0;
    });

    public readonly canGoNext = computed(() => {
        const paging = this.paging();
        if (!paging || paging.count <= 0) {
            return false;
        }

        if (paging.total < 0) {
            return paging.count === paging.pageSize;
        }

        return paging.page < Math.ceil(paging.total / paging.pageSize) - 1;
    });

    public readonly translationInfo = computed(() => {
        return {
            itemFirst: this.itemFirst(),
            itemLast: this.itemLast(),
            numberOfItems: this.paging()?.total,
        };
    });

    public goPrev() {
        const paging = this.paging();
        if (!paging) {
            return;
        }

        this.pagingChange.emit({ page: paging.page - 1, pageSize: paging.pageSize });
    }

    public goNext() {
        const paging = this.paging();
        if (!paging) {
            return;
        }

        this.pagingChange.emit({ page: paging.page + 1, pageSize: paging.pageSize });
    }

    public setPageSize(pageSize: number) {
        this.pagingChange.emit({ page: 0, pageSize });
    }
}