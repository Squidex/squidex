/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalStoreService } from './../services/local-store.service';

export class Pager {
    public canGoNext = false;
    public canGoPrev = false;

    public itemFirst = 0;
    public itemLast = 0;

    public skip = 0;

    constructor(
        public readonly numberOfItems: number,
        public readonly page = 0,
        public readonly pageSize = 10
    ) {
        const totalPages = Math.ceil(numberOfItems / this.pageSize);

        if (this.page >= totalPages && this.page > 0) {
            this.page = page = totalPages - 1;
        }

        this.itemFirst = numberOfItems === 0 ? 0 : page * this.pageSize + 1;
        this.itemLast = Math.min(numberOfItems, (page + 1) * this.pageSize);

        this.canGoNext = page < totalPages - 1;
        this.canGoPrev = page > 0;

        this.skip = page * pageSize;
    }

    public static fromLocalStore(name: string, localStore: LocalStoreService, size = 10) {
        let pageSize = localStore.getInt(`${name}.pageSize`, size);

        if (pageSize < 0 || pageSize > 100) {
            pageSize = size;
        }

        return new Pager(0, 0, pageSize);
    }

    public saveTo(name: string, localStore: LocalStoreService) {
        localStore.setInt(`${name}.pageSize`, this.pageSize);
    }

    public goNext(): Pager {
        if (!this.canGoNext) {
            return this;
        }

        return new Pager(this.numberOfItems, this.page + 1, this.pageSize);
    }

    public goPrev(): Pager {
        if (!this.canGoPrev) {
            return this;
        }

        return new Pager(this.numberOfItems, this.page - 1, this.pageSize);
    }

    public reset(): Pager {
        return new Pager(0, 0, this.pageSize);
    }

    public setPageSize(pageSize: number): Pager {
        return new Pager(this.numberOfItems, this.page, pageSize);
    }

    public setCount(numberOfItems: number): Pager {
        return new Pager(numberOfItems, this.page, this.pageSize);
    }

    public incrementCount(): Pager {
        const result = new Pager(this.numberOfItems + 1, this.page, this.pageSize);

        if (result.canGoNext) {
            result.itemLast = this.itemLast + 1;
        }

        return result;
    }

    public decrementCount(): Pager {
        const result = new Pager(this.numberOfItems - 1, this.page, this.pageSize);

        if (result.canGoNext) {
            result.itemLast = this.itemLast - 1;
        }

        return result;
    }
}