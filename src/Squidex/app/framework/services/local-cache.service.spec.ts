/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { LocalCacheService, LocalCacheServiceFactory } from './../';

describe('LocalCache', () => {
    it('should instantiate from factory', () => {
        const localCacheService = LocalCacheServiceFactory();

        expect(localCacheService).toBeDefined();
    });

    it('should instantiate', () => {
        const localCacheService = new LocalCacheService();

        expect(localCacheService).toBeDefined();
    });

    it('should get and store item in cache', () => {
        const localCacheService = new LocalCacheService();

        const value = {};

        localCacheService.set('key', value);

        expect(localCacheService.get('key')).toBe(value);
    });

    it('should get and store item in cache', () => {
        const localCacheService = new LocalCacheService();

        const value = {};

        localCacheService.set('key', value);
        localCacheService.clear(true);

        expect(localCacheService.get('key')).toBeUndefined();
    });

    it('should not retrieve item if expired', () => {
        const localCacheService = new LocalCacheService();

        const value = {};

        localCacheService.set('key', value);

        expect(localCacheService.get('key', new Date().getTime() + 400)).toBeUndefined();
    });
});