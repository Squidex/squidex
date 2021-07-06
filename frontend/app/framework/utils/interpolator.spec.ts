/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-template-curly-in-string */

import { DateTime } from './date-time';
import { interpolate } from './interpolator';

describe('interpolate', () => {
    it('should keep string untouched if does not contain interpolation', () => {
        const result = interpolate('hello world');

        expect(result).toEqual('hello world');
    });

    it('should interpolate with object value', () => {
        const result = interpolate('hello ${string}', { string: 'world' });

        expect(result).toEqual('hello world');
    });

    it('should interpolate without dollar', () => {
        const result = interpolate('hello {string}', { string: 'world' });

        expect(result).toEqual('hello world');
    });

    it('should interpolate with multiple object values', () => {
        const result = interpolate('hello ${string1}${string2}', { string1: 'world', string2: '!' });

        expect(result).toEqual('hello world!');
    });

    it('should interpolate with array value', () => {
        const result = interpolate('hello ${1}', ['my', 'world']);

        expect(result).toEqual('hello world');
    });

    it('should interpolate with complex path', () => {
        const result = interpolate('hello ${array.1}', { array: ['my', 'world'] });

        expect(result).toEqual('hello world');
    });

    it('should return undefined if not found in object', () => {
        const result = interpolate('hello ${value}', { string: 'world' });

        expect(result).toEqual('hello undefined');
    });

    it('should return undefined if not found in array', () => {
        const result = interpolate('hello ${4}', ['my', 'world']);

        expect(result).toEqual('hello undefined');
    });

    it('should return undefined if not a valid index', () => {
        const result = interpolate('hello ${index}', ['my', 'world']);

        expect(result).toEqual('hello undefined');
    });

    it('should return undefined if it resolved to object', () => {
        const result = interpolate('hello ${data}', { data: {} });

        expect(result).toEqual('hello undefined');
    });

    it('should return undefined if it resolved to array', () => {
        const result = interpolate('hello ${data}', { data: [] });

        expect(result).toEqual('hello undefined');
    });

    it('should allow object shortcuts', () => {
        const result = interpolate('hello ${data}', { data: { iv: 'world' } }, 'iv');

        expect(result).toEqual('hello world');
    });

    it('should resolve dateTime', () => {
        const now = DateTime.now();

        const result = interpolate('hello ${time}', { time: now });

        expect(result).toEqual(`hello ${now.toISOString()}`);
    });
});
