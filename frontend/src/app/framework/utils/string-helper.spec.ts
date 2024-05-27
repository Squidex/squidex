/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { StringHelper } from './string-helper';

describe('StringHelper', () => {
    it('should return empty text if value is null or undefined', () => {
        expect(StringHelper.firstNonEmpty(null!)).toBe('');
        expect(StringHelper.firstNonEmpty(undefined!)).toBe('');
    });

    it('should return fallback name if label is undefined or null', () => {
        expect(StringHelper.firstNonEmpty(null!, 'fallback')).toBe('fallback');
        expect(StringHelper.firstNonEmpty(undefined!, 'fallback')).toBe('fallback');
    });

    it('should return label if value is valid', () => {
        expect(StringHelper.firstNonEmpty('name')).toBe('name');
    });

    it('should return trimmed label if value is valid', () => {
        expect(StringHelper.firstNonEmpty(' name ')).toBe('name');
    });

    it('should return fallback name if label is empty', () => {
        expect(StringHelper.firstNonEmpty('', 'fallback')).toBe('fallback');
    });

    it('should return trimmed fallback name if label is undefined', () => {
        expect(StringHelper.firstNonEmpty('', ' fallback ')).toBe('fallback');
    });

    it('should return empty string if also fallback not found', () => {
        expect(StringHelper.firstNonEmpty(null!, undefined!, '')).toBe('');
    });

    it('should append dot if not added', () => {
        expect(StringHelper.appendLast('text', '.')).toBe('text.');
    });

    it('should not append dot if already added', () => {
        expect(StringHelper.appendLast('text.', '.')).toBe('text.');
    });

    it('should return hash for null value', () => {
        expect(StringHelper.hashCode(null!)).toBe(0);
    });

    it('should return hash for empty value', () => {
        expect(StringHelper.hashCode('')).toBe(0);
    });

    it('should return hash for concrete values', () => {
        expect(StringHelper.hashCode('ABC')).not.toBe(StringHelper.hashCode('XYZ'));
    });

    it('should build query for empty object', () => {
        const url = StringHelper.buildQuery({});

        expect(url).toEqual('');
    });

    it('should build query for single value', () => {
        const url = StringHelper.buildQuery({ key1: '42' });

        expect(url).toEqual('?key1=42');
    });

    it('should build query for multiple values', () => {
        const url = StringHelper.buildQuery({ key1: '42', key2: 21 });

        expect(url).toEqual('?key1=42&key2=21');
    });

    it('should build query and ignore null and undefined', () => {
        const url = StringHelper.buildQuery({ key1: '42', key2: 21, key: undefined, key4: null });

        expect(url).toEqual('?key1=42&key2=21');
    });

    it('should build query with encoded values', () => {
        const url = StringHelper.buildQuery({ key1: 'Hello World' });

        expect(url).toEqual('?key1=Hello%20World');
    });
});
