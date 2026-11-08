/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Injector, Signal, signal, WritableSignal } from '@angular/core';
import { required, SchemaPathTree, validate } from '@angular/forms/signals';
import { SignalForm, Types } from '@app/framework';
import { AppLanguageDto, FieldDto, SchemaDto, TableField } from '../model';
import { ContentRules } from './contents.form-rules';
import { contentTranslationStatus, FieldSection, FormGlobals, groupFields, PartitionConfig } from './contents.forms-helpers';
import { FieldForm } from './contents.forms-model';
import { ContentSchemaBuilder, toErrors } from './contents.forms-schema';
import { ContentValues, sanitizeValue } from './contents.forms-values';
import { FieldsValidators } from './contents.forms.visitors';

export * from './contents.forms-model';

type SaveQueryFormType = { name: string; user: boolean };

export class SaveQueryForm extends SignalForm<SaveQueryFormType, SaveQueryFormType> {
    constructor(injector?: Injector) {
        super({ name: '', user: false }, path => {
            required(path.name);
        }, injector);
    }
}

export class PatchContentForm extends SignalForm<Record<string, any>, any> {
    private readonly editableFields: ReadonlyArray<FieldDto>;

    public readonly dirty = computed(() => {
        return this.form().dirty();
    });

    constructor(
        listFields: ReadonlyArray<TableField>,
        private readonly language: AppLanguageDto,
        injector?: Injector,
    ) {
        const editableFields = listFields.filter(x => x.rootField?.isInlineEditable).map(x => x.rootField!);

        super(createPatchModel(editableFields), path => {
            for (const field of editableFields) {
                const validators = [...FieldsValidators.create(field, language.isOptional)];

                validate((path as SchemaPathTree<Record<string, any>>)[field.name], ctx => {
                    return toErrors(validators, ctx.value());
                });
            }
        }, injector);

        this.editableFields = editableFields;
    }

    protected transformSubmit(value: Record<string, any>) {
        const request = {} as Record<string, any>;

        for (const field of this.editableFields) {
            const fieldValue = sanitizeValue(value[field.name]);

            if (field.isLocalizable) {
                request[field.name] = { [this.language.iso2Code]: fieldValue };
            } else {
                request[field.name] = { iv: fieldValue };
            }
        }

        return request;
    }
}

export class EditContentForm extends SignalForm<any, any> {
    private readonly fields: { [name: string]: FieldForm } = {};
    private readonly context: WritableSignal<any>;
    private readonly values: ContentValues;
    private initialData: any;

    public readonly sections: ReadonlyArray<FieldSection<FieldDto, FieldForm>>;

    public readonly value = this.model.asReadonly();

    public readonly translationStatus = computed(() => {
        return contentTranslationStatus(this.model(), this.schema, this.languages);
    });

    constructor(
        public readonly languages: ReadonlyArray<AppLanguageDto>,
        public readonly schema: SchemaDto,
        schemas: { [id: string ]: SchemaDto },
        context: any,
        injector?: Injector,
    ) {
        const partitions = new PartitionConfig(languages);
        const contentValues = new ContentValues(partitions, schemas);
        const contextSignal = signal<any>(context);

        let root: Signal<any> | undefined;

        const builder = new ContentSchemaBuilder({
            context: contextSignal,
            partitions,
            root: () => root?.(),
            rules: new ContentRules(schema, schemas),
            schemas,
        });

        super(contentValues.createData(schema), path => {
            builder.build(path, schema);
        }, injector);

        root = this.model;

        this.context = contextSignal;
        this.values = contentValues;

        const globals: FormGlobals = { partitions, schema, schemas, values: contentValues };

        this.sections = groupFields(schema.fields).map(({ separator, fields }) => {
            const forms: FieldForm[] = [];

            for (const field of fields) {
                const childForm = new FieldForm({
                    field,
                    globals,
                    isOptional: false,
                    partition: '',
                    path: field.name,
                }, (this.form as any)[field.name]);

                forms.push(childForm);

                this.fields[field.name] = childForm;
            }

            return new FieldSection<FieldDto, FieldForm>(separator, forms);
        });

        this.updateInitialData();
    }

    public get(field: string | FieldDto): FieldForm | undefined {
        if (Types.is(field, FieldDto)) {
            return this.fields[field.name];
        } else {
            return this.fields[field];
        }
    }

    public hasChanged() {
        return !Types.equals(this.initialData, this.model(), true);
    }

    public hasChanges(changes: any) {
        return !Types.equals(this.initialData, changes, true);
    }

    public load(value: any, isInitial?: boolean) {
        super.load(value);

        if (isInitial) {
            this.updateInitialData();
        }
    }

    public setContext(context?: any) {
        this.context.set(context);
    }

    protected transformLoad(value: any) {
        return this.values.createData(this.schema, value);
    }

    protected transformSubmit(value: any) {
        return sanitizeValue(value);
    }

    protected onSubmitted() {
        this.updateInitialData();
    }

    private updateInitialData() {
        this.initialData = this.model();

        for (const [key, field] of Object.entries(this.fields)) {
            field.updateInitial(this.initialData?.[key]);
        }
    }
}

function createPatchModel(fields: ReadonlyArray<FieldDto>) {
    const result: Record<string, any> = {};

    for (const field of fields) {
        result[field.name] = null;
    }

    return result;
}
