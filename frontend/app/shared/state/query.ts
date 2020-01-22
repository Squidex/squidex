/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Types } from '@app/framework';

import { StatusInfo } from './../services/contents.service';
import { LanguageDto } from './../services/languages.service';
import { MetaFields, SchemaDetailsDto } from './../services/schemas.service';

export type QueryValueType =
    'boolean' |
    'date' |
    'datetime' |
    'number' |
    'reference' |
    'status' |
    'string' |
    'tags';

export interface FilterOperator {
    // The optional display value.
    name?: string;

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

    // The child filters if the logical filter is a disjunction (OR).
    or?: FilterNode[];
}

export interface QuerySorting {
    // The full path to the property.
    path: string;

    // The sort order.
    order: SortMode;
}

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

export function encodeQuery(query?: Query) {
    if (Types.isEmpty(query)) {
        return '';
    }

    query = { ...query };

    if (!query.sort) {
        query.sort = [];
    }

    if (!query.filter) {
        query.filter = { and: [] };
    }

    return encodeURIComponent(JSON.stringify(query));
}

export function hasFilter(query?: Query) {
    return !!query && !Types.isEmpty(query.filter);
}

const EqualOperators: ReadonlyArray<FilterOperator> = [
    { name: 'is equals to', value: 'eq' },
    { name: 'is not equals to', value: 'ne' }
];

const CompareOperator: ReadonlyArray<FilterOperator> = [
    { name: 'is less than', value: 'lt' },
    { name: 'is less than or equals to', value: 'le' },
    { name: 'is greater than', value: 'gt' },
    { name: 'is greater than or equals to', value: 'ge' }
];

const StringOperators: ReadonlyArray<FilterOperator> = [
    { name: 'starts with', value: 'startsWith' },
    { name: 'ends with', value: 'endsWith' },
    { name: 'contains', value: 'contains' }
];

const ArrayOperators: ReadonlyArray<FilterOperator> = [
    { value: 'is empty', noValue: true }
];

const TypeBoolean: QueryFieldModel = {
    type: 'boolean',
    operators: EqualOperators
};

const TypeDateTime: QueryFieldModel = {
    type: 'datetime',
    operators: [...EqualOperators, ...CompareOperator]
};

const TypeNumber: QueryFieldModel = {
    type: 'number',
    operators: [...EqualOperators, ...CompareOperator]
};

const TypeReference: QueryFieldModel = {
    type: 'reference',
    operators: [...EqualOperators, ...ArrayOperators]
};

const TypeStatus: QueryFieldModel = {
    type: 'status',
    operators: EqualOperators
};

const TypeString: QueryFieldModel = {
    type: 'string',
    operators: [...EqualOperators, ...CompareOperator, ...StringOperators, ...ArrayOperators]
};

const TypeTags: QueryFieldModel = {
    type: 'string',
    operators: EqualOperators
};

const DEFAULT_FIELDS: QueryModelFields = {
    created: {
        ...TypeDateTime,
        displayName: MetaFields.created,
        description: 'The date time when the content item was created.'
    },
    createdBy: {
        ...TypeString,
        displayName: 'meta.createdBy',
        description: 'The user who created the content item.'
    },
    lastModified: {
        ...TypeDateTime,
        displayName: MetaFields.lastModified,
        description: 'The date time when the content item was modified the last time.'
    },
    lastModifiedBy: {
        ...TypeString,
        displayName: 'meta.lastModifiedBy',
        description: 'The user who modified the content item the last time.'
    },
    version: {
        ...TypeNumber,
        displayName: MetaFields.version,
        description: 'The version of the content item'
    }
};

export function queryModelFromSchema(schema: SchemaDetailsDto, languages: ReadonlyArray<LanguageDto>, statuses: ReadonlyArray<StatusInfo> | undefined) {
    const languagesCodes = languages.map(x => x.iso2Code);

    const model: QueryModel = {
        fields: { ...DEFAULT_FIELDS }
    };

    if (statuses) {
        model.fields['status'] = {
             ...TypeStatus,
             displayName: MetaFields.status,
             description: 'The status of the content item.',
             extra: statuses
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
            const extra = field.rawProperties.singleId;

            type = { ...TypeReference, extra };
        }

        if (type) {
            if (field.isLocalizable) {
                for (const code of languagesCodes) {
                    const infos = {
                        displayName: `${field.name} (${code})`,
                        description: `The '${field.displayName}' field of the content item (localized).`
                    };

                    model.fields[`data.${field.name}.${code}`] = { ...type, ...infos };
                }
            } else {
                const infos = {
                    displayName: field.name,
                    description: `The '${field.displayName}' field of the content item.`
                };

                model.fields[`data.${field.name}.iv`] = { ...type, ...infos };
            }
        }
    }

    return model;
}