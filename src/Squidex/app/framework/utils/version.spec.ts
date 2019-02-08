/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Version, Versioned } from './version';

describe('Version', () => {
    it('should initialize with init value', () => {
        const version = new Version('1.0');

        expect(version.value).toBe('1.0');
    });

    it('should ignore prefix for equal comparison', () => {
        expect(new Version('2').eq(new Version('2'))).toBeTruthy();
        expect(new Version('2').eq(new Version('W/2'))).toBeTruthy();
        expect(new Version('W/2').eq(new Version('2'))).toBeTruthy();
        expect(new Version('W/2').eq(new Version('W/2'))).toBeTruthy();
    });
});

describe('Versioned', () => {
    it('should initialize with version and payload', () => {
        const versioned = new Versioned<number>(new Version('1.0'), 123);

        expect(versioned.version.value).toBe('1.0');
        expect(versioned.payload).toBe(123);
    });
});