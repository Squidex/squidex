/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from '@app/framework';
import { FieldDto, SchemaDto } from './../model';
import { AnyFieldDto, PartitionConfig } from './contents.forms-helpers';
import { contentFields } from './contents.forms-schema';
import { FieldDefaultValue } from './contents.forms.visitors';

export class ContentValues {
    constructor(
        private readonly partitions: PartitionConfig,
        private readonly schemas: { [id: string]: SchemaDto },
    ) {
    }

    public createData(schema: SchemaDto, value?: any) {
        const useDefaults = Types.isUndefined(value);
        const result: Record<string, any> = {};

        for (const field of contentFields(schema.fields)) {
            const fieldValue: Record<string, any> = {};

            for (const { key } of this.partitions.getAll(field)) {
                fieldValue[key] = this.createValue(field, value?.[field.name]?.[key], key, useDefaults);
            }

            result[field.name] = fieldValue;
        }

        return result;
    }

    public createItem(field: AnyFieldDto, partition: string) {
        if (!Types.is(field, FieldDto)) {
            return {};
        }

        return this.createObject(field.nested || [], undefined, partition, true);
    }

    public createComponent(schemaId: string, partition: string, value?: any) {
        const schema = this.schemas[schemaId];
        if (!schema) {
            return {};
        }

        return { schemaId, ...this.createObject(schema.fields, value, partition, Types.isUndefined(value)) };
    }

    public createValue(field: AnyFieldDto, value: any, partition: string, useDefaults: boolean): any {
        if (Types.isUndefined(value) && useDefaults) {
            const defaultValue = FieldDefaultValue.get(field, partition);

            return Types.isUndefined(defaultValue) ? null : defaultValue;
        }

        switch (field.properties.fieldType) {
            case 'Array':
                if (!Types.isArray(value)) {
                    return null;
                }

                return value.map(item => this.createObject((field as FieldDto).nested || [], item, partition, false));
            case 'Component':
                return this.createComponentValue(value, partition);
            case 'Components':
                if (!Types.isArray(value)) {
                    return null;
                }

                return value.map(item => this.createComponentValue(item, partition));
            default:
                return Types.isUndefined(value) ? null : value;
        }
    }

    private createComponentValue(value: any, partition: string) {
        if (!Types.isObject(value)) {
            return null;
        }

        return this.createComponent((value as any).schemaId, partition, value);
    }

    private createObject(fields: ReadonlyArray<AnyFieldDto>, value: any, partition: string, useDefaults: boolean) {
        const result: Record<string, any> = {};

        for (const field of contentFields(fields)) {
            result[field.name] = this.createValue(field, value?.[field.name], partition, useDefaults);
        }

        return result;
    }
}

export function sanitizeValue(value: any): any {
    if (typeof value === 'number' && Number.isNaN(value)) {
        return null;
    }

    if (Types.isArray(value)) {
        return value.map(sanitizeValue);
    }

    if (Types.isObject(value)) {
        const result: Record<string, any> = {};

        for (const [key, child] of Object.entries(value)) {
            result[key] = sanitizeValue(child);
        }

        return result;
    }

    return value;
}

