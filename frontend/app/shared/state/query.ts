/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { QueryParams, RouteSynchronizer, Types } from '@app/framework';
import { LanguageDto } from './../services/languages.service';
import { MetaFields, SchemaDto } from './../services/schemas.service';

export type QueryValueType =
    'boolean' |
    'date' |
    'datetime' |
    'number' |
    'reference' |
    'status' |
    'string' |
    'tags' |
    'user';

export type StatusInfo =
    Readonly<{ status: string; color: string }>;

export interface FilterOperator {
    // The optional display value.
    name: string;

    // The operator value.
    value: string;

    // True, when the operator does not require an value.
    noValue?: boolean;
}

export interface QueryFieldModel {
    // The value type.
    type: QueryValueType;

    // The allowed operator.
    operators: ReadonlyArray<FilterOperator>;

    // Extra values.
    extra?: any;

    // The optional display name for the field.
    displayName?: string;

    // The optional description for the field.
    description?: string;
}

type QueryModelFields = { [name: string]: QueryFieldModel };

export interface QueryModel {
    // All available fields.
    fields: QueryModelFields;
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

export function encodeQuery(query?: Query | null) {
    return encodeURIComponent(serializeQuery(query));
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

const EqualOperators: ReadonlyArray<FilterOperator> = [
    { name: 'i18n:common.queryOperators.eq', value: 'eq' },
    { name: 'i18n:common.queryOperators.ne', value: 'ne' },
];

const CompareOperator: ReadonlyArray<FilterOperator> = [
    { name: 'i18n:common.queryOperators.lt', value: 'lt' },
    { name: 'i18n:common.queryOperators.le', value: 'le' },
    { name: 'i18n:common.queryOperators.gt', value: 'gt' },
    { name: 'i18n:common.queryOperators.ge', value: 'ge' },
];

const StringOperators: ReadonlyArray<FilterOperator> = [
    { name: 'i18n:common.queryOperators.matchs', value: 'matchs' },
    { name: 'i18n:common.queryOperators.startsWith', value: 'startsWith' },
    { name: 'i18n:common.queryOperators.endsWith', value: 'endsWith' },
    { name: 'i18n:common.queryOperators.contains', value: 'contains' },
];

const ArrayOperators: ReadonlyArray<FilterOperator> = [
    { name: 'i18n:common.queryOperators.empty', value: 'empty', noValue: true },
    { name: 'i18n:common.queryOperators.exists', value: 'exists', noValue: true },
];

const TypeBoolean: QueryFieldModel = {
    type: 'boolean',
    operators: EqualOperators,
};

const TypeDateTime: QueryFieldModel = {
    type: 'datetime',
    operators: [...EqualOperators, ...CompareOperator],
};

const TypeNumber: QueryFieldModel = {
    type: 'number',
    operators: [...EqualOperators, ...CompareOperator],
};

const TypeReference: QueryFieldModel = {
    type: 'reference',
    operators: [...EqualOperators, ...ArrayOperators],
};

const TypeStatus: QueryFieldModel = {
    type: 'status',
    operators: EqualOperators,
};

const TypeUser: QueryFieldModel = {
    type: 'user',
    operators: EqualOperators,
};

const TypeString: QueryFieldModel = {
    type: 'string',
    operators: [...EqualOperators, ...CompareOperator, ...StringOperators, ...ArrayOperators],
};

const TypeTags: QueryFieldModel = {
    type: 'string',
    operators: EqualOperators,
};

const DEFAULT_FIELDS: QueryModelFields = {
    created: {
        ...TypeDateTime,
        displayName: MetaFields.created,
        description: 'i18n:contents.createFieldDescription',
    },
    createdBy: {
        ...TypeUser,
        displayName: 'meta.createdBy',
        description: 'i18n:contents.createdByFieldDescription',
    },
    lastModified: {
        ...TypeDateTime,
        displayName: MetaFields.lastModified,
        description: 'i18n:contents.lastModifiedFieldDescription',
    },
    lastModifiedBy: {
        ...TypeUser,
        displayName: 'meta.lastModifiedBy',
        description: 'i18n:contents.lastModifiedByFieldDescription',
    },
    version: {
        ...TypeNumber,
        displayName: MetaFields.version,
        description: 'i18n:contents.versionFieldDescription',
    },
};

export function queryModelFromSchema(schema: SchemaDto, languages: ReadonlyArray<LanguageDto>, statuses: ReadonlyArray<StatusInfo> | undefined) {
    const languagesCodes = languages.map(x => x.iso2Code);

    const model: QueryModel = {
        fields: { ...DEFAULT_FIELDS },
    };

    if (statuses) {
        model.fields['status'] = {
            ...TypeStatus,
            displayName: MetaFields.status,
            description: 'i18n:contents.statusFieldDescription',
            extra: statuses,
        };

        model.fields['newStatus'] = {
            ...TypeStatus,
            displayName: MetaFields.statusNext,
            description: 'i18n:contents.newStatusFieldDescription',
            extra: statuses,
        };
    }

    for (const field of schema.fields) {
        let type: QueryFieldModel | null = null;

        if (field.properties.fieldType === 'Tags') {
            type = TypeTags;
        } else if (field.properties.fieldType === 'Boolean') {
            type = TypeBoolean;
        } else if (field.properties.fieldType === 'Number') {
            type = TypeNumber;
        } else if (field.properties.fieldType === 'String') {
            type = TypeString;
        } else if (field.properties.fieldType === 'DateTime') {
            type = TypeDateTime;
        } else if (field.properties.fieldType === 'References') {
            const extra = field.rawProperties.schemaIds;

            type = { ...TypeReference, extra };
        }

        if (type) {
            if (field.isLocalizable) {
                for (const code of languagesCodes) {
                    const infos = {
                        displayName: `${field.name} (${code})`,
                        description: 'i18n:contents.localizedFieldDescription',
                        fieldName: field.displayName,
                    };

                    model.fields[`data.${field.name}.${code}`] = { ...type, ...infos };
                }
            } else {
                const infos = {
                    displayName: field.name,
                    description: 'i18n:contents.invariantFieldDescription',
                    fieldName: field.displayName,
                };

                model.fields[`data.${field.name}.iv`] = { ...type, ...infos };
            }
        }
    }

    return model;
}
