/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Signal } from '@angular/core';
import { applyEach, applyWhenValue, disabled, FieldContext, hidden, schema, Schema, SchemaPath, SchemaPathTree, validate, ValidationError } from '@angular/forms/signals';
import { Types, validateValue, ValueValidator } from '@app/framework';
import { FieldDto, SchemaDto } from './../model';
import { ContentRules, RuleAction } from './contents.form-rules';
import { AnyFieldDto, PartitionConfig } from './contents.forms-helpers';
import { FieldsValidators } from './contents.forms.visitors';

export type ContentSchemaGlobals = Readonly<{
    context: Signal<any>;
    partitions: PartitionConfig;
    root: () => any;
    rules: ContentRules;
    schemas: { [id: string]: SchemaDto };
}>;

type AnyPath = SchemaPath<any, any, any>;

export class ContentSchemaBuilder {
    private readonly componentSchemas = new Map<string, Schema<any>>();
    private readonly validators = new WeakMap<AnyFieldDto, ReadonlyArray<ValueValidator>[]>();

    constructor(
        private readonly globals: ContentSchemaGlobals,
    ) {
    }

    public build(path: SchemaPathTree<any>, schemaDto: SchemaDto) {
        for (const field of contentFields(schemaDto.fields)) {
            const fieldPath = (path as any)[field.name];

            for (const { key } of this.globals.partitions.getAll(field)) {
                this.applyField(fieldPath[key], field, undefined);
            }
        }
    }

    private applyField(path: AnyPath, field: AnyFieldDto, itemPath: AnyPath | undefined) {
        const isRuleActive = (ctx: FieldContext<any, any>, action: RuleAction) => {
            const root = this.globals.root();

            const rules = this.globals.rules.getRules(ctx.pathKeys(), root, field);
            if (rules.length === 0) {
                return false;
            }

            const context = { ...this.globals.context() || {}, data: root };
            const itemData = itemPath ? ctx.valueOf(itemPath) : root;

            return rules.some(x => x.action === action && x.eval(context, itemData));
        };

        disabled(path, (ctx: FieldContext<any, any>) => {
            return field.isDisabled || isRuleActive(ctx, 'Disable');
        });

        hidden(path, (ctx: FieldContext<any, any>) => {
            return isRuleActive(ctx, 'Hide');
        });

        validate(path, ctx => {
            const value = ctx.value();
            const isOptional = this.globals.partitions.isOptional(ctx.pathKeys()[1]);

            const isRequired = (field.properties.isRequired && !isOptional) || isRuleActive(ctx, 'Require');
            if (isRequired && isEmpty(value)) {
                return { kind: 'required' };
            }

            return toErrors(this.getValidators(field, isOptional), value);
        });

        const fieldType = field.properties.fieldType;

        if (fieldType === 'Array' && Types.is(field, FieldDto)) {
            applyEach(path as SchemaPath<any[]>, item => {
                for (const nested of contentFields(field.nested || [])) {
                    this.applyField((item as any)[nested.name], nested, item);
                }
            });
        } else if (fieldType === 'Component') {
            this.applyComponent(path);
        } else if (fieldType === 'Components') {
            applyEach(path as SchemaPath<any[]>, item => {
                this.applyComponent(item);
            });
        }
    }

    private applyComponent(path: AnyPath) {
        for (const [id, schemaDto] of Object.entries(this.globals.schemas)) {
            applyWhenValue(path, (value: any) => value?.schemaId === id, this.getComponentSchema(id, schemaDto));
        }
    }

    private getComponentSchema(id: string, schemaDto: SchemaDto) {
        let result = this.componentSchemas.get(id);
        if (!result) {
            result = schema<any>(path => {
                for (const field of contentFields(schemaDto.fields)) {
                    this.applyField((path as any)[field.name], field, path);
                }
            });

            this.componentSchemas.set(id, result);
        }

        return result;
    }

    private getValidators(field: AnyFieldDto, isOptional: boolean) {
        let result = this.validators.get(field);
        if (!result) {
            result = [
                FieldsValidators.createWithoutRequired(field, false),
                FieldsValidators.createWithoutRequired(field, true),
            ];

            this.validators.set(field, result);
        }

        return result[isOptional ? 1 : 0];
    }
}

export function contentFields<T extends AnyFieldDto>(fields: ReadonlyArray<T>) {
    return fields.filter(x => x.properties.isContentField);
}

export function isEmpty(value: any) {
    return Types.isUndefined(value) || Types.isNull(value) || value === '' || (Types.isArray(value) && value.length === 0);
}

export function toErrors(validators: ReadonlyArray<ValueValidator>, value: any): ValidationError.WithoutField[] {
    return validateValue(validators, value);
}
