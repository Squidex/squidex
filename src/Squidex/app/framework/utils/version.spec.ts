/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Version } from './../';

describe('Version', () => {
    it('should initialize with init value', () => {
        const version = new Version('1.0');

        expect(version.value).toBe('1.0');
    });
});