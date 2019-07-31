/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LanguageDto, SchemaDetailsDto } from '@app/shared/internal';

export type QueryValueType =
    'boolean' |
    'date' |
    'datetime' |
    'number' |
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
    operators: FilterOperator[];

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
    order: 'ascending' | 'descending';
}

export interface Query {
    // The optional filter.
    filter?: FilterNode;

    // The full text search.
    fullText?: string;

    // The sorting.
    sorting?: QuerySorting[];
}

const TypeBoolean: QueryFieldModel = {
    type: 'boolean',
    operators: [
        { value: 'eq', name: '==' },
        { value: 'ne', name: '!=' }
    ]
};

const TypeDateTime: QueryFieldModel = {
    type: 'datetime',
    operators: [
        { value: 'eq', name: '==' },
        { value: 'ne', name: '!=' },
        { value: 'lt', name: '<' },
        { value: 'le', name: '<=' },
        { value: 'gt', name: '>' },
        { value: 'ge', name: '>=' }
    ]
};

const TypeNumber: QueryFieldModel = {
    type: 'number',
    operators: [
        { value: 'eq', name: '==' },
        { value: 'ne', name: '!=' },
        { value: 'lt', name: '<' },
        { value: 'le', name: '<=' },
        { value: 'gt', name: '>' },
        { value: 'ge', name: '>=' }
    ]
};

const TypeReferences: QueryFieldModel = {
    type: 'string',
    operators: [
        { value: 'eq', name: '==' },
        { value: 'ne', name: '!=' }
    ]
};

const TypeString: QueryFieldModel = {
    type: 'string',
    operators: [
        { value: 'eq', name: '==' },
        { value: 'ne', name: '!=' },
        { value: 'lt', name: '<' },
        { value: 'le', name: '<=' },
        { value: 'gt', name: '>' },
        { value: 'ge', name: '>=' },
        { value: 'startsWith' },
        { value: 'empty' },
        { value: 'endsWith' },
        { value: 'contains' }
    ]
};

export function queryModelFromSchema(schema: SchemaDetailsDto, languages: LanguageDto[]) {
    let languagesCodes = languages.map(x => x.iso2Code);

    let invariantCodes = ['iv'];

    let model: QueryModel = {
        fields: {}
    };

    model.fields['created'] = TypeDateTime;
    model.fields['createdBy'] = TypeString;
    model.fields['lastModified'] = TypeDateTime;
    model.fields['lastModifiedBy'] = TypeString;
    model.fields['status'] = TypeString;
    model.fields['version'] = TypeNumber;

    for (let field of schema.fields) {
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
            const extra = { schemaId: field.properties['schemaId'] };

            type = { ...TypeReferences, extra };
        }

        if (type) {
            let codes = field.isLocalizable ? languagesCodes : invariantCodes;

            for (let code of codes) {
                model.fields[`data.${field.name}.${code}`] = type;
            }
        }
    }

    return model;
}