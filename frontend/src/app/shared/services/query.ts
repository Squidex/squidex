/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ComplexQuery } from 'ngx-inline-filter';
import { QueryParams, RouteSynchronizer, Types } from '@app/framework';
import { StatusInfo } from './contents.service';

export type FilterSchemaType =
    'Any' |
    'Boolean' |
    'DateTime' |
    'GeoObject' |
    'Guid' |
    'Number' |
    'Object' |
    'ObjectArray' |
    'String' |
    'StringArray';

export interface FilterSchema {
    // The value type.
    readonly type: FilterSchemaType;

    // Extra values.
    readonly extra?: any;

    // The fields.
    readonly fields: ReadonlyArray<FilterableField>;
}

export interface FilterableField {
    // The schema type.
    readonly schema: FilterSchema;

    // The field path.
    readonly path: string;

    // The optional description for the field.
    readonly description?: string;
}

export interface QueryModel {
    // All available fields.
    readonly schema: FilterSchema;

    // All available statuses.
    readonly statuses: ReadonlyArray<StatusInfo>;

    // The allowed operators.
    readonly operators: Readonly<{ [type: string]: ReadonlyArray<string> }>;
}

export interface Query extends ComplexQuery {
    // The number of items to take.
    take?: number;

    // The number of items to skip.
    skip?: number;
}

export class QueryFullTextSynchronizer implements RouteSynchronizer {
    public static readonly INSTANCE = new QueryFullTextSynchronizer();

    public readonly keys = ['query'];

    public parseFromRoute(params: QueryParams) {
        const query = params['query'];

        if (Types.isString(query)) {
            return { query: { fullText: query } };
        }

        return undefined;
    }

    public parseFromState(state: any) {
        const value = state['query'];

        if (Types.isObject(value) && Types.isString(value.fullText) && value.fullText.length > 0) {
            return { query: value.fullText };
        }

        return undefined;
    }
}

export class QuerySynchronizer implements RouteSynchronizer {
    public static readonly INSTANCE = new QuerySynchronizer();

    public readonly keys = ['query'];

    public parseFromRoute(params: QueryParams) {
        const query = params['query'];

        if (Types.isString(query)) {
            return { query: deserializeQuery(query) };
        }

        return undefined;
    }

    public parseFromState(state: any) {
        const value = state['query'];

        if (Types.isObject(value)) {
            return { query: serializeQuery(value) };
        }

        return undefined;
    }
}

export function sanitize(query?: Query | null) {
    if (!query) {
        return { filter: { and: [] }, sort: [] };
    }

    if (!query.sort) {
        query.sort = [];
    }

    if (!query.filter) {
        query.filter = { and: [] };
    }

    return query;
}

export function equalsQuery(lhs?: Query | null, rhs?: Query | null) {
    return Types.equals(sanitize(lhs), sanitize(rhs));
}

export function serializeQuery(query?: Query | null) {
    return JSON.stringify(sanitize(query));
}

export function deserializeQuery(raw?: string): Query | undefined {
    let query: Query | undefined;

    try {
        if (Types.isString(raw)) {
            if (raw.indexOf('{') === 0) {
                query = JSON.parse(raw);
            } else {
                query = { fullText: raw };
            }
        }
    } catch (ex) {
        query = undefined;
    }

    return query;
}

export function hasFilter(query?: Query | null) {
    return !!query && !Types.isEmpty(query.filter);
}
