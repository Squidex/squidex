/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ImmutableArray } from './../';

describe('ImmutableArray', () => {
    it('should create empty instance', () => {
        const array_1 = ImmutableArray.of();

        expect(array_1.length).toBe(0);
    });

    it('should create same instance for empty arrays', () => {
        const array_a = ImmutableArray.of();
        const array_b = ImmutableArray.of();
        const array_c = ImmutableArray.of([]);
        const array_d = ImmutableArray.empty();

        expect(array_b).toBe(array_a);
        expect(array_c).toBe(array_a);
        expect(array_d).toBe(array_a);
    });

    it('should create non empty instance', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);

        expect(array_1.length).toBe(3);
        expect(array_1.values).toEqual([1, 2, 3]);
    });

    it('should push items', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.push(4, 5);

        expect(array_2.length).toBe(5);
        expect(array_2.values).toEqual([1, 2, 3, 4, 5]);
    });

    it('should return same array if pushing zero items', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.push();

        expect(array_2).toBe(array_1);
    });

    it('should push front items', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.pushFront(4, 5);

        expect(array_2.length).toBe(5);
        expect(array_2.values).toEqual([4, 5, 1, 2, 3]);
    });

    it('should return same array if pushing zero items to the front', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.pushFront();

        expect(array_2).toBe(array_1);
    });

    it('should remove item', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.remove(2);

        expect(array_2.length).toBe(2);
        expect(array_2.values).toEqual([1, 3]);
    });

    it('should return same array if removing zero items', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.remove();

        expect(array_2).toBe(array_1);
    });

    it('should remove all by predicate', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.removeAll((i: number) => i % 2 === 0);

        expect(array_2.values).toEqual([1, 3]);
    });

    it('should return original if nothing has been removed', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.removeAll((i: number) => i % 200 === 0);

        expect(array_2).toEqual(array_1);
    });

    it('should replace item if found', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.replace(2, 4);

        expect(array_2.values).toEqual([1, 4, 3]);
    });

    it('should not replace item if not found', () => {
        const array_1 = ImmutableArray.of([1, 2, 3]);
        const array_2 = array_1.replace(5, 5);

        expect(array_2).toBe(array_1);
    });

    it('should replace all by predicate', () => {
        const array_1 = ImmutableArray.of([1, 2, 3, 4]);
        const array_2 = array_1.replaceAll((i: number) => i % 2 === 0, i => i * 2);

        expect(array_2.values).toEqual([1, 4, 3, 8]);
    });

    it('should replace by field', () => {
        const array_1 = ImmutableArray.of([{ id: 1, v: 1 }, { id: 2, v: 2 }]);
        const array_2 = array_1.replaceBy('id', { id: 1, v: 11 });

        expect(array_2.values).toEqual([{ id: 1, v: 11 }, { id: 2, v: 2 }]);
    });

    it('should return original if nothing has been replace', () => {
        const array_1 = ImmutableArray.of([1, 2, 3, 4]);
        const array_2 = array_1.replaceAll((i: number) => i % 200 === 0, i => i);

        expect(array_2).toBe(array_1);
    });

    it('should filter items', () => {
        const array_1 = ImmutableArray.of([1, 2, 3, 4]);
        const array_2 = array_1.filter((i: number) => i % 2 === 0);

        expect(array_2.values).toEqual([2, 4]);
    });

    it('should map items', () => {
        const array_1 = ImmutableArray.of([1, 2, 3, 4]);
        const array_2 = array_1.map((i: number) => i * 2);

        expect(array_2.values).toEqual([2, 4, 6, 8]);
    });

    it('should find item', () => {
        const array_1 = ImmutableArray.of([1, 2, 3, 4]);
        const result = array_1.find(i => i >= 2.5);

        expect(result).toEqual(3);
    });

    it('should not return item if not found', () => {
        const array_1 = ImmutableArray.of([1, 2, 3, 4]);
        const result = array_1.find(i => i >= 4.5);

        expect(result).toBeUndefined();
    });

    it('should sort items', () => {
        const array_1 = ImmutableArray.of([3, 1, 4, 2]);
        const array_2 = array_1.sort((x, y) => x - y);

        expect(array_2.values).toEqual([1, 2, 3, 4]);
    });

    it('should sort ascending by numbers', () => {
        const array_1 = ImmutableArray.of([{ id: 3 }, { id: 2 }, { id: 1 }]);
        const array_2 = array_1.sortByNumberAsc(x => x.id);

        expect(array_2.values).toEqual([{ id: 1 }, { id: 2 }, { id: 3 }]);
    });

    it('should sort descending by numbers', () => {
        const array_1 = ImmutableArray.of([{ id: 1 }, { id: 2 }, { id: 3 }]);
        const array_2 = array_1.sortByNumberDesc(x => x.id);

        expect(array_2.values).toEqual([{ id: 3 }, { id: 2 }, { id: 1 }]);
    });

    it('should sort ascending by string', () => {
        const array_1 = ImmutableArray.of([{ id: '3' }, { id: '2' }, { id: '1' }]);
        const array_2 = array_1.sortByStringAsc(x => x.id);

        expect(array_2.values).toEqual([{ id: '1' }, { id: '2' }, { id: '3' }]);
    });

    it('should sort descending by string', () => {
        const array_1 = ImmutableArray.of([{ id: '1' }, { id: '2' }, { id: '3' }]);
        const array_2 = array_1.sortByStringDesc(x => x.id);

        expect(array_2.values).toEqual([{ id: '3' }, { id: '2' }, { id: '1' }]);
    });

    it('should provide mutable values', () => {
        const array_1 = ImmutableArray.of([3, 1, 4, 2]);

        expect(array_1.mutableValues).toBe(array_1.mutableValues);
    });

    it('should iterate over array items', () => {
        const array_1 = ImmutableArray.of([3, 1, 4, 2]);

        const values: number[] = [];

        for (let iter = array_1[Symbol.iterator](), _step: any; !(_step = iter.next()).done; ) {
            values.push(_step.value);
        }

        expect(values).toEqual([3, 1, 4, 2]);
    });
});

