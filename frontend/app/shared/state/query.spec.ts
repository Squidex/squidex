/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Params } from '@angular/router';
import { Query } from '@app/shared/internal';
import { equalsQuery, QueryFullTextSynchronizer, QuerySynchronizer } from './query';

describe('equalsQuery', () => {
    it('should return true when comparing with empty query', () => {
        const lhs: Query = {};

        const rhs: Query = {
            filter: {
                and: []
            },
            sort: []
        };

        expect(equalsQuery(lhs, rhs)).toBeTruthy();
    });

    it('should return true when comparing without sort', () => {
        const lhs: Query = {
            filter: {
                and: []
            }
        };

        const rhs: Query = {
            filter: {
                and: []
            },
            sort: []
        };

        expect(equalsQuery(lhs, rhs)).toBeTruthy();
    });

    it('should return true when comparing without filter', () => {
        const lhs: Query = {
            sort: []
        };

        const rhs: Query = {
            filter: {
                and: []
            },
            sort: []
        };

        expect(equalsQuery(lhs, rhs)).toBeTruthy();
    });
});

describe('QueryFullTextSynchronizer', () => {
    const synchronizer = new QueryFullTextSynchronizer();

    it('should write full text to route', () => {
        const params: Params = {};

        const value = { fullText: 'my-query' };

        synchronizer.writeValuesToRoute(value, params);

        expect(params).toEqual({ query: 'my-query' });
    });

    it('Should write undefined when not a query', () => {
        const params: Params = {};

        const value = 123;

        synchronizer.writeValuesToRoute(value, params);

        expect(params).toEqual({ query: undefined });
    });

    it('Should write undefined query has no full text', () => {
        const params: Params = {};

        const value = { fullText: '' };

        synchronizer.writeValuesToRoute(value, params);

        expect(params).toEqual({ query: undefined });
    });

    it('should get query from route', () => {
        const params: Params = {
            query: 'my-query'
        };

        const value = synchronizer.parseValuesFromRoute(params);

        expect(value).toEqual({ query: { fullText: 'my-query' } });
    });

    it('should get query as undefined from route', () => {
        const params: Params = {};

        const value = synchronizer.parseValuesFromRoute(params);

        expect(value).toEqual({ query: undefined });
    });
});

describe('QuerySynchronizer', () => {
    const synchronizer = new QuerySynchronizer();

    it('should write query to route', () => {
        const params: Params = {};

        const value = { filter: 'my-filter' };

        synchronizer.writeValuesToRoute(value, params);

        expect(params).toEqual({ query: '{"filter":"my-filter","sort":[]}' });
    });

    it('Should write undefined when not a query', () => {
        const params: Params = {};

        const value = 123;

        synchronizer.writeValuesToRoute(value, params);

        expect(params).toEqual({ query: undefined });
    });

    it('should get query from route', () => {
        const params: Params = {
            query: '{"filter":"my-filter"}'
        };

        const value = synchronizer.parseValuesFromRoute(params) as any;

        expect(value).toEqual({ query: { filter: 'my-filter' } });
    });

    it('should get query full text from route', () => {
        const params: Params = {
            query: 'my-query'
        };

        const value = synchronizer.parseValuesFromRoute(params);

        expect(value).toEqual({ query: { fullText: 'my-query' } });
    });

    it('should get query as undefined from route', () => {
        const params: Params = {};

        const value = synchronizer.parseValuesFromRoute(params);

        expect(value).toEqual({ query: undefined });
    });
});