/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { allData, allParams } from './router-utils';

describe('RouterUtils', () => {
    it('should concat all params from route', () => {
        const snapshot = {
            params: {
                key1: 'key1',
            },
            parent: {
                params: {
                    key1: 'key1-parent',
                    key2: 'key2',
                },
            },
        };

        const params = allParams(<any>{ snapshot });

        expect(params).toEqual({
            key1: 'key1',
            key2: 'key2',
        });
    });

    it('should concat all params from snapshot', () => {
        const snapshot = {
            params: {
                key1: 'key1',
            },
            parent: {
                params: {
                    key1: 'key1-parent',
                    key2: 'key2',
                },
            },
        };

        const params = allParams(<any>snapshot);

        expect(params).toEqual({
            key1: 'key1',
            key2: 'key2',
        });
    });
    it('should concat all data from route', () => {
        const snapshot = {
            data: {
                key1: 'key1',
            },
            parent: {
                data: {
                    key1: 'key1-parent',
                    key2: 'key2',
                },
            },
        };

        const params = allData(<any>{ snapshot });

        expect(params).toEqual({
            key1: 'key1',
            key2: 'key2',
        });
    });

    it('should concat all data from snapshot', () => {
        const snapshot = {
            data: {
                key1: 'key1',
            },
            parent: {
                data: {
                    key1: 'key1-parent',
                    key2: 'key2',
                },
            },
        };

        const params = allData(<any>snapshot);

        expect(params).toEqual({
            key1: 'key1',
            key2: 'key2',
        });
    });
});
