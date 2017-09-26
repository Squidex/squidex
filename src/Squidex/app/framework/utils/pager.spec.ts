/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Pager } from './../';

describe('Pager', () => {
    it('should init with default values', () => {
        const pager_1 = new Pager(0);

        expect(Object.assign({}, pager_1)).toEqual({
            page: 0,
            pageSize: 10,
            itemFirst: 0,
            itemLast: 0,
            skip: 0,
            numberOfItems: 0,
            canGoNext: false,
            canGoPrev: false
        });
    });

    it('should init with page size and page', () => {
        const pager_1 = new Pager(23, 2, 10);

        expect(Object.assign({}, pager_1)).toEqual({
            page: 2,
            pageSize: 10,
            itemFirst: 21,
            itemLast: 23,
            skip: 20,
            numberOfItems: 23,
            canGoNext: false,
            canGoPrev: true
        });
    });

    it('should reset page on reset', () => {
        const pager_1 = new Pager(23, 2, 10);
        const pager_2 = pager_1.reset();

        expect(Object.assign({}, pager_2)).toEqual({
            page: 0,
            pageSize: 10,
            itemFirst: 0,
            itemLast: 0,
            skip: 0,
            numberOfItems: 0,
            canGoNext: false,
            canGoPrev: false
        });
    });

    it('should return same instance when go next and being on last page', () => {
        const pager_1 = new Pager(23, 2, 10);
        const pager_2 = pager_1.goNext();

        expect(pager_2).toBe(pager_1);
    });

    it('should update page when going next', () => {
        const pager_1 = new Pager(23, 0, 10);
        const pager_2 = pager_1.goNext();

        expect(Object.assign({}, pager_2)).toEqual({
            page: 1,
            pageSize: 10,
            itemFirst: 11,
            itemLast: 20,
            skip: 10,
            numberOfItems: 23,
            canGoNext: true,
            canGoPrev: true
        });
    });

    it('should return same instance when go prev and being on first page', () => {
        const pager_1 = new Pager(23, 0, 10);
        const pager_2 = pager_1.goPrev();

        expect(pager_2).toBe(pager_1);
    });

    it('should update page when going prev', () => {
        const pager_1 = new Pager(23, 2, 10);
        const pager_2 = pager_1.goPrev();

        expect(Object.assign({}, pager_2)).toEqual({
            page: 1,
            pageSize: 10,
            itemFirst: 11,
            itemLast: 20,
            skip: 10,
            numberOfItems: 23,
            canGoNext: true,
            canGoPrev: true
        });
    });

    it('should update count when setting it', () => {
        const pager_1 = new Pager(23, 2, 10);
        const pager_2 = pager_1.setCount(30);

        expect(Object.assign({}, pager_2)).toEqual({
            page: 2,
            pageSize: 10,
            itemFirst: 21,
            itemLast: 30,
            skip: 20,
            numberOfItems: 30,
            canGoNext: false,
            canGoPrev: true
        });
    });

    it('should update count when incrementing it', () => {
        const pager_1 = new Pager(23, 2, 10);
        const pager_2 = pager_1.incrementCount();

        expect(Object.assign({}, pager_2)).toEqual({
            page: 2,
            pageSize: 10,
            itemFirst: 21,
            itemLast: 24,
            skip: 20,
            numberOfItems: 24,
            canGoNext: false,
            canGoPrev: true
        });
    });

    it('should update count when decrementing it', () => {
        const pager_1 = new Pager(23, 2, 10);
        const pager_2 = pager_1.decrementCount();

        expect(Object.assign({}, pager_2)).toEqual({
            page: 2,
            pageSize: 10,
            itemFirst: 21,
            itemLast: 22,
            skip: 20,
            numberOfItems: 22,
            canGoNext: false,
            canGoPrev: true
        });
    });

    it('should also update page when new page is bigger than max page', () => {
        const pager_1 = new Pager(21, 2, 10);
        const pager_2 = pager_1.decrementCount();

        expect(Object.assign({}, pager_2)).toEqual({
            page: 1,
            pageSize: 10,
            itemFirst: 11,
            itemLast: 20,
            skip: 10,
            numberOfItems: 20,
            canGoNext: false,
            canGoPrev: true
        });
    });
});