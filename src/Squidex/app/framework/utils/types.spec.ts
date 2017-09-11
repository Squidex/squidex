/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Types } from './../';

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
});