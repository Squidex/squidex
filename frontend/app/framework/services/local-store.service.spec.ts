/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalStoreService } from './local-store.service';

describe('LocalStore', () => {
    it('should instantiate', () => {
        const localStoreService = new LocalStoreService();

        expect(localStoreService).toBeDefined();
    });

    it('should call local store for set function', () => {
        const localStoreService = new LocalStoreService();

        let passedKey = '';
        let passedVal = '';

        localStoreService.configureStore({
            setItem: (k: string, v: string) => {
                passedKey = k;
                passedVal = v;
            },
        });

        localStoreService.set('mykey', 'myval');

        expect(passedKey).toBe('mykey');
        expect(passedVal).toBe('myval');
    });

    it('should call local store for get function', () => {
        const localStoreService = new LocalStoreService();

        let passedKey = '';

        localStoreService.configureStore({
            getItem: (key: string): string => {
                passedKey = key;

                return 'myval';
            },
        });

        const returnedVal = localStoreService.get('mykey');

        expect(passedKey).toBe('mykey');
        expect(returnedVal).toBe('myval');
    });

    it('should set and get from fallback value if local store failed', () => {
        const localStoreService = new LocalStoreService();

        localStoreService.configureStore({
            setItem: () => {
                throw new Error('Not supported');
            },
            getItem: () => {
                throw new Error('Not supported');
            },
        });

        localStoreService.set('mykey', 'myval');

        expect(localStoreService.get('mykey')).toBe('myval');
    });

    it('should get boolean from local store', () => {
        const localStoreService = new LocalStoreService();

        localStoreService.setBoolean('key1', true);
        localStoreService.setBoolean('key2', false);

        expect(localStoreService.getBoolean('key1')).toBe(true);
        expect(localStoreService.getBoolean('key2')).toBe(false);

        expect(localStoreService.getBoolean('not_set')).toBe(false);
    });

    it('should get int from local store', () => {
        const localStoreService = new LocalStoreService();

        localStoreService.set('key1', 'abc');
        localStoreService.setInt('key2', 2);
        localStoreService.setInt('key3', 0);

        expect(localStoreService.getInt('key1', 13)).toBe(13);
        expect(localStoreService.getInt('key2', 13)).toBe(2);
        expect(localStoreService.getInt('key3', 13)).toBe(0);

        expect(localStoreService.getInt('not_set', 13)).toBe(13);
    });

    it('should remove item from local store', () => {
        const localStoreService = new LocalStoreService();

        localStoreService.set('key1', 'abc');
        localStoreService.remove('key1');

        expect(localStoreService.get('key1')).toBeNull();
    });
});
