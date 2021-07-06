/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/naming-convention */

import './array-extensions';

describe('ArrayExtensions', () => {
    describe('replacedBy', () => {
        it('should return same array if value is not defined', () => {
            const array_0 = [{ id: 1 }, { id: 2 }];
            const array_1 = array_0.replacedBy('id', null!);

            expect(array_1).toBe(array_0);
        });

        it('should return new array with new value', () => {
            const array_0 = [{ id: 1, v: 10 }, { id: 2, v: 20 }];
            const array_1 = array_0.replacedBy('id', { id: 1, v: 30 });

            expect(array_1).not.toBe(array_0);
            expect(array_1).toEqual([{ id: 1, v: 30 }, { id: 2, v: 20 }]);
        });
    });

    describe('replaceBy', () => {
        it('should return same array if value is not defined', () => {
            const array_0 = [{ id: 1 }, { id: 2 }];
            const array_1 = array_0.replaceBy('id', null!);

            expect(array_1).toBe(array_0);
        });

        it('should return array with new value', () => {
            const array_0 = [{ id: 1, v: 10 }, { id: 2, v: 20 }];
            const array_1 = array_0.replaceBy('id', { id: 1, v: 30 });

            expect(array_1).toBe(array_0);
            expect(array_1).toEqual([{ id: 1, v: 30 }, { id: 2, v: 20 }]);
        });
    });

    describe('removed', () => {
        it('should return new array without removed value', () => {
            const array_0 = [1, 2, 3];
            const array_1 = array_0.removed(2);

            expect(array_1).not.toBe(array_0);
            expect(array_1).toEqual([1, 3]);
        });
    });

    describe('remove', () => {
        it('should return same array without removed value', () => {
            const array_0 = [1, 2, 3];
            const array_1 = array_0.remove(2);

            expect(array_1).toBe(array_0);
            expect(array_1).toEqual([1, 3]);
        });
    });

    describe('removedBy', () => {
        it('should return new array if value is not defined', () => {
            const array_0 = [{ id: 1 }, { id: 2 }];
            const array_1 = array_0.removedBy('id', null!);

            expect(array_1).toBe(array_0);
        });

        it('should return new array without removed value', () => {
            const array_0 = [{ id: 1 }, { id: 2 }];
            const array_1 = array_0.removedBy('id', { id: 1 });

            expect(array_1).not.toBe(array_0);
            expect(array_1).toEqual([{ id: 2 }]);
        });
    });

    describe('removeBy', () => {
        it('should return same array if value is not defined', () => {
            const array_0 = [{ id: 1 }, { id: 2 }];
            const array_1 = array_0.removeBy('id', null!);

            expect(array_1).toBe(array_0);
        });

        it('should return same array without removed value', () => {
            const array_0 = [{ id: 1 }, { id: 2 }];
            const array_1 = array_0.removeBy('id', { id: 1 });

            expect(array_1).toBe(array_0);
            expect(array_1).toEqual([{ id: 2 }]);
        });
    });

    it('should sort by value', () => {
        const array_0 = [3, 1, 2];
        const array_1 = array_0.sorted();

        expect(array_1).toEqual([1, 2, 3]);
    });

    it('should sort by property and create new value', () => {
        const array_0 = [{ id: 'C' }, { id: 'b' }, { id: 'A' }];
        const array_1 = array_0.sortedByString(x => x.id);

        expect(array_1).not.toBe(array_0);
        expect(array_1).toEqual([{ id: 'A' }, { id: 'b' }, { id: 'C' }]);
    });

    it('should sort by property with same value', () => {
        const array_0 = [{ id: 'C' }, { id: 'b' }, { id: 'A' }];
        const array_1 = array_0.sortByString(x => x.id);

        expect(array_1).toBe(array_0);
        expect(array_1).toEqual([{ id: 'A' }, { id: 'b' }, { id: 'C' }]);
    });

    it('should convert to map', () => {
        const array_0 = [{ id: 'A', value: 1 }, { id: 'B', value: 2 }, { id: 'B', value: 3 }];
        const map = array_0.toMap(x => x.id);

        expect(map).toEqual({
            A: { id: 'A', value: 1 },
            B: { id: 'B', value: 3 },
        });
    });
});
