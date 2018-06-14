/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Lazy } from './lazy';

describe('Lazy', () => {
    it('should provider value', () => {
        const lazy = new Lazy(() => 1);

        expect(lazy.value).toBe(1);
    });

    it('should call delegate once', () => {
        let called = 0;

        const lazy = new Lazy(() => {
            called++;
            return 13;
        });

        expect(lazy.value).toBe(13);
        expect(lazy.value).toBe(13);
        expect(called).toBe(1);
    });

    it('should call delegate once when returned undefined', () => {
        let called = 0;

        const lazy = new Lazy(() => {
            called++;
            return undefined;
        });

        expect(lazy.value).toBeUndefined();
        expect(lazy.value).toBeUndefined();
        expect(called).toBe(1);
    });
});