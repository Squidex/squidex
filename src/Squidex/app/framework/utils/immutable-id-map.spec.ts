/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ImmutableIdMap } from './../';

class MockupData {
    public readonly id: string;

    constructor(public readonly value: string | null, id?: string) {
        this.id = id || value!;
    }
}

describe('ImmutableIdMap', () => {
    const v1 = new MockupData('value1');
    const v2 = new MockupData('value2');
    const v3 = new MockupData('value3');
    const v4 = new MockupData('value4');
    const v5 = new MockupData('value5');
    const v6 = new MockupData('value6');

    it('should instantiate without arguments', () => {
        const list = new ImmutableIdMap<MockupData>();

        expect(list).toBeDefined();
    });

    it('should instantiate from array of items', () => {
        const list_1 = new ImmutableIdMap<MockupData>([v1, v2, v3]);

        expect(list_1.size).toBe(3);
        expect(list_1.get('value1')).toBe(v1);
        expect(list_1.get('value2')).toBe(v2);
        expect(list_1.get('value3')).toBe(v3);

        expect(list_1.first).toBe(v1);
        expect(list_1.last).toBe(v3);
    });

    it('should add values to list', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.add(v2);
        const list_4 = list_3.add(v3);

        expect(list_4.size).toBe(3);
        expect(list_4.get('value1')).toBe(v1);
        expect(list_4.get('value2')).toBe(v2);
        expect(list_4.get('value3')).toBe(v3);
    });

    it('should convert to array', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.add(v2);
        const list_4 = list_3.add(v3);

        const items = list_4.toArray();

        expect(items.length).toBe(3);
        expect(items[0]).toBe(v1);
        expect(items[1]).toBe(v2);
        expect(items[2]).toBe(v3);
    });

    it('should return original list when value to add has no id', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(new MockupData(null));

        expect(list_2).toBe(list_1);
    });

    it('should return original list when value to add is null', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(null!);

        expect(list_2).toBe(list_1);
    });

    it('should return original list when item to add has already been added', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.add(v1);

        expect(list_3).toBe(list_2);
    });

    it('should update item', () => {
        const newValue = new MockupData(v1.value, v1.id);

        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(v1.id, t => newValue);

        expect(list_3.size).toBe(1);
        expect(list_3.get('value1')).toBe(newValue);
    });

    it('should return undefined for invalid id', () => {
        const list_1 = new ImmutableIdMap<MockupData>();

        expect(list_1.get(null!)).toBeUndefined();
    });

    it('should return original list when id to update is null', () => {
        const newValue = new MockupData(v1.value, v1.id);

        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(null!, t => newValue);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when id to update does not exists', () => {
        const newValue = new MockupData(v1.value, v1.id);

        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update('unknown', t => newValue);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when updater is null', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(v1.id, null!);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when updater returns same item', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(v1.id, t => t);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when updater returns item with another id', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.update(v1.id, t => v2);

        expect(list_3).toBe(list_2);
    });

    it('should remove values from list', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.add(v2);
        const list_4 = list_3.remove('value1');

        expect(list_4.size).toBe(1);
        expect(list_4.get('value2')).toBe(v2);
        expect(list_4.contains(v1.id)).toBeFalsy();
    });

    it('should return original list when id to remove is null', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.remove(v1.id, null!);

        expect(list_3).toBe(list_2);
    });

    it('should return original list when id to remove does not exists', () => {
        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(v1);
        const list_3 = list_2.remove(v1.id, 'unknown');

        expect(list_3).toBe(list_2);
    });

    it('should bring to front', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.bringToFront([v3.id, v5.id]);

        expect(list_2.map(t => t.id)).toEqual([v1, v2, v4, v6, v3, v5].map(t => t.id));
    });

    it('should bring forwards', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.bringForwards([v3.id, v4.id]);

        expect(list_2.map(t => t.id)).toEqual([v1, v2, v5, v3, v4, v6].map(t => t.id));
    });

    it('should send to back', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendToBack([v3.id, v5.id]);

        expect(list_2.map(t => t.id)).toEqual([v3, v5, v1, v2, v4, v6].map(t => t.id));
    });

    it('should send backwards', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendBackwards([v3.id, v5.id]);

        expect(list_2.map(t => t.id)).toEqual([v1, v3, v5, v2, v4, v6].map(t => t.id));
    });

    it('should move item', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.moveTo([v4.id], 1);

        expect(list_2.map(t => t.id)).toEqual([v1, v4, v2, v3, v5, v6].map(t => t.id));
    });

    it('should ignore items that are not found', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.bringToFront([v3.id, 'not found']);

        expect(list_2.map(t => t.id)).toEqual([v1, v2, v4, v5, v6, v3].map(t => t.id));
    });

    it('should return original list no id found', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendBackwards(['not found', 'other not found']);

        expect(list_2).toBe(list_1);
    });

    it('should return original list when ids is null', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);
        const list_2 = list_1.sendBackwards(null!);

        expect(list_2).toBe(list_1);
    });

    it('should return correct result for map', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);

        const result = list_1.map(t => t.id);

        expect(result).toEqual(['value1', 'value2', 'value3', 'value4', 'value5', 'value6']);
    });

    it('should return correct result for forEach', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);

        const result: string[] = [];

        list_1.forEach(t => result.push(t.id));

        expect(result).toEqual(['value1', 'value2', 'value3', 'value4', 'value5', 'value6']);
    });

    it('should return correct result for filter', () => {
        const list_1 = new ImmutableIdMap<MockupData>().add(v1, v2, v3, v4, v5, v6);

        let i = 0;

        const result = list_1.filter(t => { i++; return i % 2 === 1; });

        expect(result).toEqual([v1, v3, v5]);
    });

    it('should add and remove large item set', () => {
        const size = 1000;
        const items: MockupData[] = [];

        for (let i = 0; i < size; i++) {
            items.push(new MockupData('id' + i));
        }

        const list_1 = new ImmutableIdMap<MockupData>();
        const list_2 = list_1.add(...items);

        expect(list_2.toArray()).toEqual(items);

        const list_3 = list_2.remove(...items.map(t => t.id));

        expect(list_3.size).toEqual(0);
    });
});
