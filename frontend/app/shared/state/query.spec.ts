/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Query } from '@app/shared/internal';
import { equalsQuery } from './query';

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