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

    [
        { src: '', result: '' },
        { src: 'M', result: 'm' },
        { src: 'My', result: 'my' },
        { src: 'M-y', result: 'mY' },
        { src: 'MyProperty ', result: 'myProperty' },
        { src: 'My property', result: 'myProperty' },
        { src: 'My_property', result: 'myProperty' },
        { src: 'My-property', result: 'myProperty' }
    ].forEach(test => {
        it(`should return convert ${test.src} to camel case`, () => {
            expect(StringHelper.toCamelCase(test.src)).toBe(test.result);
        });
    });
});