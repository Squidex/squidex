/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { QueryParams, RouteSynchronizer, Types } from '@app/framework';

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

export type FilterFieldUI =
    'Boolean' |
    'Date' |
    'DateTime' |
    'Reference' |
    'None' |
    'Number' |
    'Select' |
    'String' |
    'Status' |
    'Unsupported' |
    'User';

export function getFilterUI(comparison: FilterComparison, field: FilterableField): FilterFieldUI {
    if (!field || !comparison) {
        return 'None';
    }

    const { type, extra } = field.schema;

    if (comparison.op === 'empty' || comparison.op === 'exists') {
        return 'None';
    } else if (type === 'Boolean') {
        return 'Boolean';
    } else if (type === 'DateTime' && extra?.editor === 'Date') {
        return 'Date';
    } else if (type === 'DateTime') {
        return 'DateTime';
    } else if (type === 'Number') {
        return 'Number';
    } else if (type === 'String' && extra?.editor === 'Status') {
        return 'Status';
    } else if (type === 'String' && extra?.editor === 'User') {
        return 'User';
    } else if (type === 'String' && extra?.options) {
        return 'Select';
    } else if (type === 'String') {
        return 'String';
    } else if (type === 'StringArray' && extra?.schemaIds) {
        return 'Reference';
    } else if (type === 'StringArray') {
        return 'String';
    } else {
        return 'Unsupported';
    }
}

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

    // The allowed operators.
    readonly operators: Readonly<{ [type: string]: ReadonlyArray<string> }>;
}

export type FilterNode = FilterComparison | FilterLogical;

export interface FilterComparison {
    // The full path to the property.
    path: string;

    // The operator.
    op: string;

    // The value.
    value: any;
}

export interface FilterLogical {
    // The child filters if the logical filter is a conjunction (AND).
    and?: FilterNode[];

    // The child filters if the logical filter is a conjunction (AND).
    or?: FilterNode[];
}

export interface QuerySorting {
    // The full path to the property.
    path: string;

    // The sort order.
    order: SortMode;
}

export const SORT_MODES: ReadonlyArray<SortMode> = [
    'ascending',
    'descending',
];

export type SortMode = 'ascending' | 'descending';

export interface Query {
    // The optional filter.
    filter?: FilterLogical;

    // The full text search.
    fullText?: string;

    // The sorting.
    sort?: QuerySorting[];

    // The number of items to take.
    take?: number;

    // The number of items to skip.
    skip?: number;
}

const DEFAULT_QUERY = {
    filter: {
        and: [],
    },
    sort: [],
};

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
        return DEFAULT_QUERY;
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
