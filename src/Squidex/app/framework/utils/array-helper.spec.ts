/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ArrayHelper } from './../';

describe('ArrayHelper', () => {
    it('should push item', () => {
        const oldArray = [1, 2, 3];
        const newArray = ArrayHelper.push(oldArray, 4);

        expect(newArray).toEqual([1, 2, 3, 4]);
    });

    it('should remove item if found', () => {
        const oldArray = [1, 2, 3];
        const newArray = ArrayHelper.remove(oldArray, 2);

        expect(newArray).toEqual([1, 3]);
    });

    it('should not remove item if not found', () => {
        const oldArray = [1, 2, 3];
        const newArray = ArrayHelper.remove(oldArray, 5);

        expect(newArray).toEqual([1, 2, 3]);
    });

    it('should remove all by predicate', () => {
        const oldArray: number[] = [1, 2, 3, 4];
        const newArray = ArrayHelper.removeAll(oldArray, (i: number) => i % 2 === 0);

        expect(newArray).toEqual([1, 3]);
    });

    it('should return original if nothing has been removed', () => {
        const oldArray: number[] = [1, 2, 3, 4];
        const newArray = ArrayHelper.removeAll(oldArray, (i: number) => i % 200 === 0);

        expect(newArray).toEqual(oldArray);
    });

    it('should replace item if found', () => {
        const oldArray = [1, 2, 3];
        const newArray = ArrayHelper.replace(oldArray, 2, 4);

        expect(newArray).toEqual([1, 4, 3]);
    });

    it('should not replace item if not found', () => {
        const oldArray = [1, 2, 3];
        const newArray = ArrayHelper.replace(oldArray, 5, 5);

        expect(newArray).toEqual([1, 2, 3]);
    });

    it('should replace all by predicate', () => {
        const oldArray: number[] = [1, 2, 3, 4];
        const newArray = ArrayHelper.replaceAll(oldArray, (i: number) => i % 2 === 0, i => i * 2);

        expect(newArray).toEqual([1, 4, 3, 8]);
    });

    it('should return original if nothing has been replace', () => {
        const oldArray: number[] = [1, 2, 3, 4];
        const newArray = ArrayHelper.replaceAll(oldArray, (i: number) => i % 200 === 0, i => i);

        expect(newArray).toEqual(oldArray);
    });

});