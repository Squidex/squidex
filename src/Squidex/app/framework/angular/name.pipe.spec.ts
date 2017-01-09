/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { DisplayNamePipe } from './name.pipe';

describe('DisplayNamePipe', () => {
    it('should return empty text if value is null or undefined', () => {
        const pipe = new DisplayNamePipe();

        expect(pipe.transform(null)).toBe('');
        expect(pipe.transform(undefined)).toBe('');
    });

    it('should return label if value is valid', () => {
        const pipe = new DisplayNamePipe();

        expect(pipe.transform({ label: 'name' })).toBe('name');
    });

    it('should return trimmed label if value is valid', () => {
        const pipe = new DisplayNamePipe();

        expect(pipe.transform({ label: ' name ' })).toBe('name');
    });

    it('should return fallback name if label is empty', () => {
        const pipe = new DisplayNamePipe();

        expect(pipe.transform({ label: ' ', name: 'fallback' })).toBe('fallback');
    });

    it('should return fallback name if label is undefined', () => {
        const pipe = new DisplayNamePipe();

        expect(pipe.transform({ name: 'fallback' })).toBe('fallback');
    });

    it('should return trimmed fallback name if label is undefined', () => {
        const pipe = new DisplayNamePipe();

        expect(pipe.transform({ name: ' fallback ' })).toBe('fallback');
    });

    it('should return empty string if also fallback not found', () => {
        const pipe = new DisplayNamePipe();

        expect(pipe.transform({ })).toBe('');
    });
});