/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

describe('Types', () => {
    it('should calculate hash string', () => {
        expect(Types.hash(null)).toBe('null');
        expect(Types.hash(undefined)).toBeUndefined();

        expect(Types.hash(new RegExp('.*'))).toEqual('{}');
    });

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

    it('should make json equals check', () => {
        expect(Types.jsJsonEquals({ a: 1, b: 2 }, { a: 1, b: 2 })).toBeTruthy();

        expect(Types.jsJsonEquals({ a: 1, b: 2 }, { b: 2, a: 1 })).toBeFalsy();
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
});

class MyClass {
    constructor(
        public readonly value: number
    ) {
    }
}