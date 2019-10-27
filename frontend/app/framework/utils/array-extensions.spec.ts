/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

describe('ArrayExtensions', () => {
    it('should return same array when replaying by property with null value', () => {
        const array_0 = [{ id: 1 }, { id: 2 }];
        const array_1 = array_0.replaceBy('id', null!);

        expect(array_1).toBe(array_0);
    });

    it('should return new array when replaying by property', () => {
        const array_0 = [{ id: 1, v: 10 }, { id: 2, v: 20 }];
        const array_1 = array_0.replaceBy('id', { id: 1, v: 30 });

        expect(array_1).toEqual([{ id: 1, v: 30 }, { id: 2, v: 20 }]);
    });

    it('should return same array when removing by property with null value', () => {
        const array_0 = [{ id: 1 }, { id: 2 }];
        const array_1 = array_0.removeBy('id', null!);

        expect(array_1).toBe(array_0);
    });

    it('should return new array when removing by property', () => {
        const array_0 = [{ id: 1 }, { id: 2 }];
        const array_1 = array_0.removeBy('id', { id: 1 });

        expect(array_1).toEqual([{ id: 2 }]);
    });

    it('should return same array when removing with null value', () => {
        const array_0 = [1, 2, 3];
        const array_1 = array_0.removed(null!);

        expect(array_1).toBe(array_0);
    });

    it('should return new array when removing', () => {
        const array_0 = [1, 2, 3];
        const array_1 = array_0.removed(2);

        expect(array_1).toEqual([1, 3]);
    });

    it('should sort by value', () => {
        const array_0 = [3, 1, 2];
        const array_1 = array_0.sorted();

        expect(array_1).toEqual([1, 2, 3]);
    });

    it('should sort by property', () => {
        const array_0 = [{ id: 'C' }, { id: 'b' }, { id: 'A' }];
        const array_1 = array_0.sortedByString(x => x.id);

        expect(array_1).toEqual([{ id: 'A' }, { id: 'b' }, { id: 'C' }]);
    });
});