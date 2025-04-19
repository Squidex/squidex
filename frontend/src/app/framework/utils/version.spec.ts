/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { VersionTag } from './version';

describe('Version', () => {
    it('should initialize with init value', () => {
        const version = new VersionTag('1.0');

        expect(version.value).toBe('1.0');
    });

    it('should ignore prefix for equal comparison', () => {
        expect(new VersionTag('2').eq(new VersionTag('2'))).toBeTruthy();
        expect(new VersionTag('2').eq(new VersionTag('W/2'))).toBeTruthy();
        expect(new VersionTag('W/2').eq(new VersionTag('2'))).toBeTruthy();
        expect(new VersionTag('W/2').eq(new VersionTag('W/2'))).toBeTruthy();
    });
});
