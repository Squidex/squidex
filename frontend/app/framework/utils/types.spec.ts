/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

describe('Types', () => {
    it('should make string check', () => {
        expect(Types.isString('')).toBeTruthy();
        expect(Types.isString('string')).toBeTruthy();

        expect(Types.isString(false)).toBeFalsy();
    });

    it('should make number check', () => {
        expect(Types.isNumber(0)).toBeTruthy();
        expect(Types.isNumber(1)).toBeTruthy();

        expect(Types.isNumber(NaN)).toBeFalsy();
        expect(Types.isNumber(Infinity)).toBeFalsy();
        expect(Types.isNumber(false)).toBeFalsy();
    });

    it('should make boolean check', () => {
        expect(Types.isBoolean(true)).toBeTruthy();
        expect(Types.isBoolean(false)).toBeTruthy();

        expect(Types.isBoolean(0)).toBeFalsy();
        expect(Types.isBoolean(1)).toBeFalsy();
    });

    it('should make number array check', () => {
        expect(Types.isArrayOfNumber([])).toBeTruthy();
        expect(Types.isArrayOfNumber([0, 1])).toBeTruthy();

        expect(Types.isArrayOfNumber(['0', 1])).toBeFalsy();
    });

    it('should make string array check', () => {
        expect(Types.isArrayOfString([])).toBeTruthy();
        expect(Types.isArrayOfString(['0', '1'])).toBeTruthy();

        expect(Types.isArrayOfString(['0', 1])).toBeFalsy();
    });

    it('should make array check', () => {
        expect(Types.isArray([])).toBeTruthy();
        expect(Types.isArray([0])).toBeTruthy();

        expect(Types.isArray({})).toBeFalsy();
    });

    it('should make object check', () => {
        expect(Types.isObject({})).toBeTruthy();
        expect(Types.isObject({ v: 1 })).toBeTruthy();

        expect(Types.isObject([])).toBeFalsy();
    });

    it('should make RegExp check', () => {
        expect(Types.isRegExp(/[.*]/)).toBeTruthy();

        expect(Types.isRegExp('/[.*]/')).toBeFalsy();
    });

    it('should make Date check', () => {
        expect(Types.isDate(new Date())).toBeTruthy();

        expect(Types.isDate(new Date().getDate())).toBeFalsy();
    });

    it('should make undefined check', () => {
        expect(Types.isUndefined(undefined)).toBeTruthy();

        expect(Types.isUndefined(null)).toBeFalsy();
    });

    it('should make null check', () => {
        expect(Types.isNull(null)).toBeTruthy();

        expect(Types.isNull(undefined)).toBeFalsy();
    });

    it('should make function check', () => {
        expect(Types.isFunction(() => { /* NOOP */ })).toBeTruthy();

        expect(Types.isFunction([])).toBeFalsy();
    });

    it('should make type check', () => {
        expect(Types.is(new MyClass(1), MyClass)).toBeTruthy();

        expect(Types.is(1, MyClass)).toBeFalsy();
    });

    it('should not treat zero as empty', () => {
        expect(Types.isEmpty(0)).toBeFalsy();
    });

    it('should not treat empty string as empty', () => {
        expect(Types.isEmpty('')).toBeFalsy();
    });

    it('should not treat false as empty', () => {
        expect(Types.isEmpty(false)).toBeFalsy();
    });

    it('should not treat array with at least one non-empty value as empty', () => {
        expect(Types.isEmpty([null, 0])).toBeFalsy();
    });

    it('should not treat array with at least one non-empty value as empty', () => {
        expect(Types.isEmpty({ a: null, b: 0 })).toBeFalsy();
    });

    it('should treat empty object as empty', () => {
        expect(Types.isEmpty({})).toBeTruthy();
    });

    it('should treat array object as empty', () => {
        expect(Types.isEmpty([])).toBeTruthy();
    });

    it('should treat null as empty', () => {
        expect(Types.isEmpty(null)).toBeTruthy();
    });

    it('should treat undefined as empty', () => {
        expect(Types.isEmpty(undefined)).toBeTruthy();
    });

    it('should treat array of empty values as empty', () => {
        expect(Types.isEmpty([])).toBeTruthy();
    });

    it('should treat object of empty values as empty', () => {
        expect(Types.isEmpty({ a: null, b: null })).toBeTruthy();
    });

    it('should compare undefined', () => {
        expect(Types.equals(undefined, undefined)).toBeTruthy();
    });

    it('should compare null', () => {
        expect(Types.equals(null, null)).toBeTruthy();
    });

    it('should compare invalid', () => {
        expect(Types.equals(null, undefined)).toBeFalsy();
    });

    it('should compare scalars', () => {
        expect(Types.equals(1, false)).toBeFalsy();
        expect(Types.equals(1, 2)).toBeFalsy();
        expect(Types.equals(2, 2)).toBeTruthy();
    });

    it('should compare arrays', () => {
        expect(Types.equals([1, 2], [2, 3])).toBeFalsy();
        expect(Types.equals([1, 2], [1, 2])).toBeTruthy();
    });

    it('should compare objects', () => {
        expect(Types.equals({ a: 1, b: 2 }, { a: 2, b: 3 })).toBeFalsy();
        expect(Types.equals({ a: 1, b: 2 }, { a: 1, b: 2 })).toBeTruthy();
    });

    it('should compare nested objects', () => {
        expect(Types.equals({ a: [1, 2] }, { a: [2, 3] })).toBeFalsy();
        expect(Types.equals({ a: [1, 2] }, { a: [1, 2] })).toBeTruthy();
    });

    const FalsyValues = [false, null, 0];

    it('should compare empty string with undefined', () => {
        expect(Types.equals('', undefined, true)).toBeTruthy();
        expect(Types.equals('', undefined, false)).toBeFalsy();

        expect(Types.equals(undefined, '', true)).toBeTruthy();
        expect(Types.equals(undefined, '', false)).toBeFalsy();
    });

    FalsyValues.forEach(x => {
        it('should compare empty string with {x}', () => {
            expect(Types.equals('', x, true)).toBeFalsy();
            expect(Types.equals('', x, false)).toBeFalsy();

            expect(Types.equals(x, '', true)).toBeFalsy();
            expect(Types.equals(x, '', false)).toBeFalsy();
        });
    });

    it('should clone array', () => {
        const source = [1, 2, 3];
        const result = Types.clone(source);

        expect(result).toEqual(source);
        expect(result).not.toBe(source);
    });

    it('should compare arrays', () => {
        const source = 13;
        const result = Types.clone(source);

        expect(result).toEqual(source);
    });

    it('should clone value', () => {
        const source = 13;
        const result = Types.clone(source);

        expect(result).toEqual(source);
    });

    it('should clone object', () => {
        const source = { a: 1, b: 2 };
        const result = Types.clone(source);

        expect(result).toEqual(source);
        expect(result).not.toBe(source);
    });

    it('should clone object of array', () => {
        const source = { a: [1, 2], b: [3, 4] };
        const result = Types.clone(source);

        expect(result).toEqual(source);
        expect(result).not.toBe(source);
        expect(result.a).not.toBe(source.a);
        expect(result.b).not.toBe(source.b);
    });

    it('should merge deeply', () => {
        const source = {};

        Types.mergeInto(source, {
            rootShared: 1,
            rootA: 2,
            nested: {
                a: 3,
            },
            array: [4],
        });

        Types.mergeInto(source, {
            rootShared: 5,
            rootB: 6,
            nested: {
                b: 7,
            },
            array: [8],
        });

        expect(source).toEqual({
            rootShared: 5,
            rootA: 2,
            rootB: 6,
            nested: {
                a: 3,
                b: 7,
            },
            array: [4, 8],
        });
    });
});

class MyClass {
    constructor(
        public readonly value: number,
    ) {
    }
}
