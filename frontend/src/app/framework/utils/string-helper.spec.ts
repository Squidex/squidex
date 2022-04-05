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

    it('should append query string to url if url already contains query', () => {
        const url = StringHelper.appendToUrl('http://squidex.io?query=value', 'other', 1);

        expect(url).toEqual('http://squidex.io?query=value&other=1');
    });

    it('should append query string to url if url already contains no query', () => {
        const url = StringHelper.appendToUrl('http://squidex.io', 'other', 1);

        expect(url).toEqual('http://squidex.io?other=1');
    });
});
