/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ImmutableSet } from './../';

describe('ImmutableSet', () => {
    it('should instantiate instance from array', () => {
        const set_1 = new ImmutableSet<string>(['1', '1', '2', '3']);

        expect(set_1.size).toBe(3);
        expect(set_1.contains('1')).toBeTruthy();
        expect(set_1.contains('2')).toBeTruthy();
        expect(set_1.contains('3')).toBeTruthy();
    });

    it('should add items', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.add('1');
        const set_4 = set_3.add('2');
        const set_5 = set_4.add('3');

        expect(set_5.size).toBe(3);
        expect(set_5.contains('1')).toBeTruthy();
        expect(set_5.contains('2')).toBeTruthy();
        expect(set_5.contains('3')).toBeTruthy();
    });

    it('should convert to aray', () => {
        const set_1 = new ImmutableSet<string>(['a', 'b']);

        const array = set_1.toArray();
        expect(array.length).toBe(2);
        expect(array.indexOf('a') >= 0).toBeTruthy();
        expect(array.indexOf('b') >= 0).toBeTruthy();
    });

    it('should return original set when item to add is null', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add(null!);

        expect(set_2).toBe(set_1);
    });

    it('should return original set when item to add already exists', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.add('1');

        expect(set_3).toBe(set_2);
    });

    it('should remove item', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.remove('1');

        expect(set_3.size).toBe(0);
    });

    it('should return original set when item to remove is not found', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.remove('unknown');

        expect(set_3).toBe(set_2);
    });

    it('should create new set', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.set(['a', 'b']);

        expect(set_3.size).toBe(2);
        expect(set_3.contains('a')).toBeTruthy();
        expect(set_3.contains('b')).toBeTruthy();
    });

    it('should return original set when any item to set is null', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.set(['1', null!]);

        expect(set_3).toBe(set_2);
    });

    it('should return original set when items to set is null', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.set(null!);

        expect(set_3).toBe(set_2);
    });

    it('should return original set when items is same', () => {
        const set_1 = new ImmutableSet<string>();
        const set_2 = set_1.add('1');
        const set_3 = set_2.set(['a', 'b']);
        const set_4 = set_3.set(['a', 'b']);

        expect(set_4).toBe(set_3);
    });

    it('should remvoe many', () => {
        const set_1 = new ImmutableSet<string>(['1', '2', '3', '4']);
        const set_2 = set_1.remove('2', '4');

        expect(set_2.size).toBe(2);
        expect(set_2.contains('1')).toBeTruthy();
        expect(set_2.contains('3')).toBeTruthy();
    });

    it('should return original set when any item to remove is null', () => {
        const set_1 = new ImmutableSet<string>(['1', '2', '3', '4']);
        const set_2 = set_1.remove('3', null!);

        expect(set_2).toBe(set_1);
    });

    it('should return original set when items to remove is null', () => {
        const set_1 = new ImmutableSet<string>(['1', '2', '3', '4']);
        const set_2 = set_1.remove(null!);

        expect(set_2).toBe(set_1);
    });

    it('should return correct result for map', () => {
        const set_1 = new ImmutableSet<string>(['1', '2', '3', '4']);

        const result = set_1.map(t => t + t);

        expect(result).toEqual(['11', '22', '33', '44']);
    });

    it('should return correct result for forEach', () => {
        const set_1 = new ImmutableSet<string>(['1', '2', '3', '4']);

        const result: string[] = [];

        set_1.forEach(t => result.push(t + t));

        expect(result).toEqual(['11', '22', '33', '44']);
    });

    it('should return correct result for filter', () => {
        const set_1 = new ImmutableSet<string>(['1', '2', '3', '4']);

        let i = 0;

        const result = set_1.filter(t => { i++; return i % 2 === 1; });

        expect(result).toEqual(['1', '3']);
    });
});
