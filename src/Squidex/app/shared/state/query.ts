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
import { SchemaDetailsDto } from './../services/schemas.service';

export type QueryValueType =
    'boolean' |
    'date' |
    'datetime' |
    'number' |
    'reference' |
    'status' |
    'string';

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
}

export interface QueryModel {
    // All available fields.
    fields: { [name: string]: QueryFieldModel };
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
    { name: '==', value: 'eq' },
    { name: '!=', value: 'ne' }
];

const CompareOperator: ReadonlyArray<FilterOperator> = [
    { name: '<', value: 'lt' },
    { name: '<=', value: 'le' },
    { name: '>', value: 'gt' },
    { name: '>=', value: 'ge' }
];

const StringOperators: ReadonlyArray<FilterOperator> = [
    { name: 'T*', value: 'startsWith' },
    { name: '*T', value: 'endsWith' },
    { name: '*T*', value: 'contains' }
];

const ArrayOperators: ReadonlyArray<FilterOperator> = [
    { value: 'empty', noValue: true }
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

export function queryModelFromSchema(schema: SchemaDetailsDto, languages: ReadonlyArray<LanguageDto>, statuses: ReadonlyArray<StatusInfo> | undefined) {
    const languagesCodes = languages.map(x => x.iso2Code);

    const invariantCodes = ['iv'];

    const model: QueryModel = {
        fields: {}
    };

    model.fields['created'] = TypeDateTime;
    model.fields['createdBy'] = TypeString;
    model.fields['lastModified'] = TypeDateTime;
    model.fields['lastModifiedBy'] = TypeString;
    model.fields['version'] = TypeNumber;

    if (statuses) {
        model.fields['status'] = { ...TypeStatus, extra: statuses };
    }

    for (const field of schema.fields) {
        let type: QueryFieldModel | null = null;

        if (field.properties.fieldType === 'Boolean') {
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
            const codes = field.isLocalizable ? languagesCodes : invariantCodes;

            for (const code of codes) {
                model.fields[`data.${field.name}.${code}`] = type;
            }
        }
    }

    return model;
}