/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalStoreService } from './local-store.service';

describe('LocalStore', () => {
    it('should instantiate', () => {
        const localStore = new LocalStoreService();

        expect(localStore).toBeDefined();
    });

    it('should call local store for set function', () => {
        const localStore = new LocalStoreService();

        let passedKey = '';
        let passedVal = '';

        localStore.configureStore({
            setItem: (k: string, v: string) => {
                passedKey = k;
                passedVal = v;
            },
        });

        localStore.set('mykey', 'myval');

        expect(passedKey).toBe('mykey');
        expect(passedVal).toBe('myval');
    });

    it('should call local store for get function', () => {
        const localStore = new LocalStoreService();

        let passedKey = '';

        localStore.configureStore({
            getItem: (key: string): string => {
                passedKey = key;

                return 'myval';
            },
        });

        const returnedVal = localStore.get('mykey');

        expect(passedKey).toBe('mykey');
        expect(returnedVal).toBe('myval');
    });

    it('should set and get from fallback value if local store failed', () => {
        const localStore = new LocalStoreService();

        localStore.configureStore({
            setItem: () => {
                throw new Error('Not supported');
            },
            getItem: () => {
                throw new Error('Not supported');
            },
        });

        localStore.set('mykey', 'myval');

        expect(localStore.get('mykey')).toBe('myval');
    });

    it('should get boolean from local store', () => {
        const localStore = new LocalStoreService();

        localStore.setBoolean('key1', true);
        localStore.setBoolean('key2', false);

        expect(localStore.getBoolean('key1')).toBe(true);
        expect(localStore.getBoolean('key2')).toBe(false);

        expect(localStore.getBoolean('not_set')).toBe(false);
    });

    it('should get int from local store', () => {
        const localStore = new LocalStoreService();

        localStore.set('key1', 'abc');
        localStore.setInt('key2', 2);
        localStore.setInt('key3', 0);

        expect(localStore.getInt('key1', 13)).toBe(13);
        expect(localStore.getInt('key2', 13)).toBe(2);
        expect(localStore.getInt('key3', 13)).toBe(0);

        expect(localStore.getInt('not_set', 13)).toBe(13);
    });

    it('should remove item from local store', () => {
        const localStore = new LocalStoreService();

        localStore.set('key1', 'abc');
        localStore.remove('key1');

        expect(localStore.get('key1')).toBeNull();
    });

    it('should provide boolean signal that writes to local store', () => {
        const localStore = new LocalStoreService();

        const value = localStore.boolean('key1');

        value.set(true);

        expect(value()).toBe(true);
        expect(localStore.getBoolean('key1')).toBe(true);
    });

    it('should update signal when value is set', () => {
        const localStore = new LocalStoreService();

        const value = localStore.int('key1', 13);

        localStore.setInt('key1', 42);

        expect(value()).toBe(42);
    });

    it('should keep signals of same key in sync', () => {
        const localStore = new LocalStoreService();

        const value1 = localStore.value('key1');
        const value2 = localStore.value('key1');

        value1.set('abc');

        expect(value2()).toBe('abc');
    });
});
