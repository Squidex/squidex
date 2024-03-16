/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { PagerComponent } from './pager.component';

describe('Pager', () => {
    it('should init with default values', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 0, pageSize: 10, count: 0, total: 0 };
        pager.ngOnChanges();

        expect(pager.canGoNext).toBeFalse();
        expect(pager.canGoPrev).toBeFalse();

        expect(pager.itemFirst).toEqual(0);
        expect(pager.itemLast).toEqual(0);
    });

    it('should init with first full page', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 0, pageSize: 10, count: 10, total: 100 };
        pager.ngOnChanges();

        expect(pager.canGoNext).toBeTrue();
        expect(pager.canGoPrev).toBeFalse();

        expect(pager.itemFirst).toEqual(1);
        expect(pager.itemLast).toEqual(10);
    });
    it('should init with middle page', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 4, pageSize: 10, count: 10, total: 100 };
        pager.ngOnChanges();

        expect(pager.canGoNext).toBeTrue();
        expect(pager.canGoPrev).toBeTrue();

        expect(pager.itemFirst).toEqual(41);
        expect(pager.itemLast).toEqual(50);
    });

    it('should init with last full page', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 9, pageSize: 10, count: 10, total: 100 };
        pager.ngOnChanges();

        expect(pager.canGoNext).toBeFalse();
        expect(pager.canGoPrev).toBeTrue();

        expect(pager.itemFirst).toEqual(91);
        expect(pager.itemLast).toEqual(100);
    });

    it('should init with last partly page', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 9, pageSize: 10, count: 4, total: 100 };
        pager.ngOnChanges();

        expect(pager.canGoNext).toBeFalse();
        expect(pager.canGoPrev).toBeTrue();

        expect(pager.itemFirst).toEqual(91);
        expect(pager.itemLast).toEqual(94);
    });

    it('should init with last partly page 2', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 9, pageSize: 10, count: 9, total: 100 };
        pager.ngOnChanges();

        expect(pager.canGoNext).toBeFalse();
        expect(pager.canGoPrev).toBeTrue();

        expect(pager.itemFirst).toEqual(91);
        expect(pager.itemLast).toEqual(99);
    });

    it('should emit if changing size', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 4, pageSize: 10, count: 10, total: 100 };
        pager.ngOnChanges();

        let emitted: any;

        pager.pagingChange.subscribe((value: any) => {
            emitted = value;
        });

        pager.setPageSize(20);

        expect(emitted).toEqual({ page: 0, pageSize: 20 });
    });

    it('should emit if going next', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 4, pageSize: 10, count: 10, total: 100 };
        pager.ngOnChanges();

        let emitted: any;

        pager.pagingChange.subscribe((value: any) => {
            emitted = value;
        });

        pager.goNext();

        expect(emitted).toEqual({ page: 5, pageSize: 10 });
    });

    it('should emit if going prev', () => {
        const pager = new PagerComponent();

        pager.paging = { page: 4, pageSize: 10, count: 10, total: 100 };
        pager.ngOnChanges();

        let emitted: any;

        pager.pagingChange.subscribe((value: any) => {
            emitted = value;
        });

        pager.goPrev();

        expect(emitted).toEqual({ page: 3, pageSize: 10 });
    });
});
