/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Version } from './version';

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
