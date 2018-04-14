/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalStoreService, LocalStoreServiceFactory } from './local-store.service';

describe('LocalStore', () => {
    it('should instantiate from factory', () => {
        const localStoreService = LocalStoreServiceFactory();

        expect(localStoreService).toBeDefined();
    });

    it('should instantiate', () => {
        const localStoreService = new LocalStoreService();

        expect(localStoreService).toBeDefined();
    });

    it('should call local store for set function', () => {
        const localStoreService = new LocalStoreService();

        let passedKey = '', passedVal = '';

        localStoreService.configureStore({
            setItem: (k: string, v: string) => {
                passedKey = k;
                passedVal = v;
            }
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
            }
        });

        const returnedVal = localStoreService.get('mykey');

        expect(passedKey).toBe('mykey');
        expect(returnedVal).toBe('myval');
    });

    it('should set and get from fallback value when local store failed', () => {
        const localStoreService = new LocalStoreService();

        localStoreService.configureStore({
            setItem: (k: string, v: string) => {
                throw 'Not supported';
            },
            getItem: (k: string) => {
                throw 'Not supported';
            }
        });

        localStoreService.set('mykey', 'myval');

        expect(localStoreService.get('mykey')).toBe('myval');
    });
});
