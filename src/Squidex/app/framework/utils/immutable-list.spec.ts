/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ImmutableList } from './../';

describe('ImmutableList', () => {
    const v1 = 'value1';
    const v2 = 'value2';
    const v3 = 'value3';
    const v4 = 'value4';
    const v5 = 'value5';
    const v6 = 'value6';

    it('should instantiate without arguments', () => {
        const list = new ImmutableList<string>();

        expect(list).toBeDefined();
    });

    it('should instantiate from array of items', () => {
        const list_1 = new ImmutableList<string>([v1, v2, v3]);

        expect(list_1.size).toBe(3);
        expect(list_1.get(0)).toBe(v1);
        expect(list_1.get(1)).toBe(v2);
        expect(list_1.get(2)).toBe(v3);
    });

    it('should add values to list', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.add(v2);
        const list_4 = list_3.add(v3);

        expect(list_4.size).toBe(3);
        expect(list_4.get(0)).toBe(v1);
        expect(list_4.get(1)).toBe(v2);
        expect(list_4.get(2)).toBe(v3);
    });

    it('should convert to array', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.add(v2);
        const list_4 = list_3.add(v3);

        const items = list_4.toArray();

        expect(items.length).toBe(3);
        expect(items[0]).toBe(v1);
        expect(items[1]).toBe(v2);
        expect(items[2]).toBe(v3);
    });

    it('should return original list when value to add is null', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(null!);

        expect(list_2).toBe(list_1);
    });

    it('should update item', () => {
        const newValue = 'v1New';

        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(v1, t => newValue);

        expect(list_3.size).toBe(1);
        expect(list_3.get(0)).toBe(newValue);
    });

    it('should return undefined for invalid index', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);

        expect(list_2.get(-10)).toBeUndefined();
        expect(list_2.get(100)).toBeUndefined();
    });

    it('should return original list when item to update is null', () => {
        const newValue = 'v1New';

        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(null!, t => newValue);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when updater is null', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(v1, null!);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when updater returns same item', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(v1, t => t);

        expect(list_3).toBe(list_2);
    });

    it('should remove values from list', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.add(v2);
        const list_4 = list_3.remove(v1);

        expect(list_4.size).toBe(1);
        expect(list_4.get(0)).toBe(v2);
    });

    it('should return original list when item to remove is null', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.remove(null!);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when item to remove does not exists', () => {
        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.remove(v4);

        expect(list_3).toBe(list_2);
    });

    it('should bring to front', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.bringToFront([v3, v5]);

        expect(list_2.toArray()).toEqual([v1, v2, v4, v6, v3, v5]);
    });

    it('should bring forwards', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.bringForwards([v3, v4]);

        expect(list_2.toArray()).toEqual([v1, v2, v5, v3, v4, v6]);
    });

    it('should send to back', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendToBack([v3, v5]);

        expect(list_2.toArray()).toEqual([v3, v5, v1, v2, v4, v6]);
    });

    it('should send backwards', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendBackwards([v3, v5]);

        expect(list_2.toArray()).toEqual([v1, v3, v5, v2, v4, v6]);
    });

    it('should move item', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.moveTo([v4], 1);

        expect(list_2.toArray()).toEqual([v1, v4, v2, v3, v5, v6]);
    });

    it('should ignore items that are not found', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.bringToFront([v3, 'not found']);

        expect(list_2.toArray()).toEqual([v1, v2, v4, v5, v6, v3]);
    });

    it('should return original list no id found', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendBackwards(['not found']);

        expect(list_2).toBe(list_1);
    });

    it('should return original list when ids is null', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendBackwards(null!);

        expect(list_2).toBe(list_1);
    });

    it('should return correct result for map', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);

        const result = list_1.map(t => 'x_' + t);

        expect(result).toEqual(['x_value1', 'x_value2', 'x_value3', 'x_value4', 'x_value5', 'x_value6']);
    });

    it('should return correct result for forEach', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);

        const result: string[] = [];

        list_1.forEach(t => result.push('x_' + t));

        expect(result).toEqual(['x_value1', 'x_value2', 'x_value3', 'x_value4', 'x_value5', 'x_value6']);
    });

    it('should return correct result for filter', () => {
        const list_1 = new ImmutableList<string>().add(v1, v2, v3, v4, v5, v6);

        let i = 0;

        const result = list_1.filter(t => { i++; return i % 2 === 1; });

        expect(result).toEqual([v1, v3, v5]);
    });

    it('should add and remove large item set', () => {
        const size = 1000;
        const items: string[] = [];

        for (let i = 0; i < size; i++) {
            items.push('id' + i);
        }

        const list_1 = new ImmutableList<string>();
        const list_2 = list_1.add(...items);

        expect(list_2.toArray()).toEqual(items);

        const list_3 = list_2.remove(...items);

        expect(list_3.size).toEqual(0);
    });
});
