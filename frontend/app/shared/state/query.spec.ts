/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Query, QueryParams } from '@app/shared/internal';
import { equalsQuery, QueryFullTextSynchronizer, QuerySynchronizer } from './query';

describe('equalsQuery', () => {
    it('should return true if comparing with empty query', () => {
        const lhs: Query = {};

        const rhs: Query = {
            filter: {
                and: [],
            },
            sort: [],
        };

        expect(equalsQuery(lhs, rhs)).toBeTruthy();
    });

    it('should return true if comparing without sort', () => {
        const lhs: Query = {
            filter: {
                and: [],
            },
        };

        const rhs: Query = {
            filter: {
                and: [],
            },
            sort: [],
        };

        expect(equalsQuery(lhs, rhs)).toBeTruthy();
    });

    it('should return true if comparing without filter', () => {
        const lhs: Query = {
            sort: [],
        };

        const rhs: Query = {
            filter: {
                and: [],
            },
            sort: [],
        };

        expect(equalsQuery(lhs, rhs)).toBeTruthy();
    });
});

describe('QueryFullTextSynchronizer', () => {
    const synchronizer = new QueryFullTextSynchronizer();

    it('should parse from state', () => {
        const value = { fullText: 'my-query' };

        const query = synchronizer.parseFromState({ query: value });

        expect(query).toEqual({ query: 'my-query' });
    });

    it('should parse from state as undefined if not a query', () => {
        const value = 123;

        const query = synchronizer.parseFromState({ query: value });

        expect(query).toBeUndefined();
    });

    it('should parse from state as undefined if no full text', () => {
        const value = { fullText: undefined };

        const query = synchronizer.parseFromState({ query: value });

        expect(query).toBeUndefined();
    });

    it('should parse from state as undefined if empty full text', () => {
        const value = { fullText: '' };

        const query = synchronizer.parseFromState({ query: value });

        expect(query).toBeUndefined();
    });

    it('should get query from route', () => {
        const params: QueryParams = { query: 'my-query' };

        const value = synchronizer.parseFromRoute(params);

        expect(value).toEqual({ query: { fullText: 'my-query' } });
    });

    it('should get query as undefined from route', () => {
        const params: QueryParams = {};

        const value = synchronizer.parseFromRoute(params);

        expect(value).toBeUndefined();
    });
});

describe('QuerySynchronizer', () => {
    const synchronizer = new QuerySynchronizer();

    it('should parse from state', () => {
        const value = { filter: 'my-filter' };

        const query = synchronizer.parseFromState({ query: value });

        expect(query).toEqual({ query: '{"filter":"my-filter","sort":[]}' });
    });
    it('should parse from state as undefined if not a query', () => {
        const value = 123;

        const query = synchronizer.parseFromState({ query: value });

        expect(query).toBeUndefined();
    });

    it('should get query from route', () => {
        const params: QueryParams = { query: '{"filter":"my-filter"}' };

        const value = synchronizer.parseFromRoute(params) as any;

        expect(value).toEqual({ query: { filter: 'my-filter' } });
    });

    it('should get query full text from route', () => {
        const params: QueryParams = { query: 'my-query' };

        const value = synchronizer.parseFromRoute(params);

        expect(value).toEqual({ query: { fullText: 'my-query' } });
    });

    it('should get query as undefined from route', () => {
        const params: QueryParams = {};

        const value = synchronizer.parseFromRoute(params);

        expect(value).toBeUndefined();
    });
});
