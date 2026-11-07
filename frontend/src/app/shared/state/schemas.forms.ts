/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { applyEach, maxLength, required } from '@angular/forms/signals';
import { SignalForm, SignalValidators } from '@app/framework';
import { AddFieldDto, ConfigureFieldRulesDto, createProperties, CreateSchemaDto, FieldRuleAction, FieldRuleDto, FieldType, SchemaDto, SchemaPropertiesDto, SchemaScriptsDto, SchemaType, SynchronizeSchemaDto, UpdateSchemaDto } from '../model';
import { loadWithDefaults, submitWithoutEmpty } from './schemas.forms-helpers';

type CreateCategoryFormModel = { name: string };

export class CreateCategoryForm extends SignalForm<CreateCategoryFormModel, CreateCategoryFormModel> {
    constructor() {
        super({ name: '' });
    }
}

type CreateSchemaFormModel = { name: string; type: SchemaType; initialCategory: string | null; importing: object };

export class CreateSchemaForm extends SignalForm<CreateSchemaFormModel, CreateSchemaDto> {
    constructor() {
        super({ name: '', type: 'Default', initialCategory: null, importing: {} }, path => {
            maxLength(path.name, 40);
            SignalValidators.patternWithMessage(path.name, '[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:schemas.schemaNameValidationMessage');
            required(path.name);
            required(path.type);
        });
    }

    protected transformLoad(value: Partial<CreateSchemaDto>): CreateSchemaFormModel {
        const { name, type, category, ...importing } = value;

        return { name: name ?? '', type: type ?? 'Default', importing, initialCategory: category ?? null };
    }

    protected transformSubmit(value: CreateSchemaFormModel) {
        const { name, type, importing, initialCategory } = value;

        return CreateSchemaDto.fromJSON({ name, type, category: initialCategory ?? undefined, ...importing });
    }
}

type GenerateSchemaFormModel = { prompt: string };

export class GenerateSchemaForm extends SignalForm<GenerateSchemaFormModel, GenerateSchemaFormModel> {
    constructor() {
        super({ prompt: '' }, path => {
            required(path.prompt);
        });
    }
}

type SynchronizeSchemaFormModel = { json: object; fieldsDelete: boolean; fieldsRecreate: boolean };

export class SynchronizeSchemaForm extends SignalForm<SynchronizeSchemaFormModel, SynchronizeSchemaDto> {
    constructor() {
        super({ json: {}, fieldsDelete: false, fieldsRecreate: false });
    }

    public loadSchema(schema: SchemaDto) {
        this.model.update(model => ({ ...model, json: schema.export() }));
    }

    protected transformSubmit(value: SynchronizeSchemaFormModel) {
        return SynchronizeSchemaDto.fromJSON({
            ...value.json,
            noFieldDeletion: !value.fieldsDelete,
            noFieldRecreation: !value.fieldsRecreate,
        });
    }
}

type FieldRuleFormModel = { action: FieldRuleAction; field: string; condition: string };

export class ConfigureFieldRulesForm extends SignalForm<FieldRuleFormModel[], ConfigureFieldRulesDto, SchemaDto> {
    constructor() {
        super([], path => {
            applyEach(path, rule => {
                required(rule.action);
                required(rule.field);
            });
        });
    }

    public add(field?: string) {
        this.model.update(rules => [...rules, { action: 'Disable', field: field ?? '', condition: '' }]);
    }

    public remove(index: number) {
        this.model.update(rules => rules.filter((_, i) => i !== index));
    }

    protected transformLoad(value: Partial<SchemaDto>): FieldRuleFormModel[] {
        return (value.fieldRules || []).map(({ action, field, condition }) => ({ action, field, condition: condition ?? '' }));
    }

    protected transformSubmit(value: FieldRuleFormModel[]) {
        return new ConfigureFieldRulesDto({ fieldRules: value.map(x => new FieldRuleDto(x)) });
    }
}

type PreviewUrlFormModel = { name: string; url: string };

export class ConfigurePreviewUrlsForm extends SignalForm<PreviewUrlFormModel[], Record<string, string>, SchemaDto> {
    constructor() {
        super([], path => {
            applyEach(path, previewUrl => {
                maxLength(previewUrl.name, 1000);
                required(previewUrl.name);
                required(previewUrl.url);
            });
        });
    }

    public add() {
        this.model.update(previewUrls => [...previewUrls, { name: '', url: '' }]);
    }

    public remove(index: number) {
        this.model.update(previewUrls => previewUrls.filter((_, i) => i !== index));
    }

    protected transformLoad(value: Partial<SchemaDto>): PreviewUrlFormModel[] {
        const result: PreviewUrlFormModel[] = [];

        if (value.previewUrls) {
            for (const [name, url] of Object.entries(value.previewUrls)) {
                result.push({ name, url });
            }
        }

        return result;
    }

    protected transformSubmit(value: PreviewUrlFormModel[]) {
        const result: Record<string, string> = {};

        for (const item of value) {
            result[item.name] = item.url;
        }

        return result;
    }
}

type SchemaScriptsFormModel = { query: string; queryPre: string; create: string; change: string; delete: string; update: string; index: string };

const SCHEMA_SCRIPTS_DEFAULTS: SchemaScriptsFormModel = { query: '', queryPre: '', create: '', change: '', delete: '', update: '', index: '' };

export class EditSchemaScriptsForm extends SignalForm<SchemaScriptsFormModel, Record<string, string>, SchemaScriptsDto> {
    public readonly scripts = Object.keys(SCHEMA_SCRIPTS_DEFAULTS) as ReadonlyArray<keyof SchemaScriptsFormModel>;

    constructor() {
        super(SCHEMA_SCRIPTS_DEFAULTS);
    }

    protected transformLoad(value: Partial<SchemaScriptsDto>): SchemaScriptsFormModel {
        return loadWithDefaults(value, SCHEMA_SCRIPTS_DEFAULTS);
    }
}

type EditSchemaFormModel = {
    label: string;
    hints: string;
    contentsSidebarUrl: string;
    contentSidebarUrl: string;
    contentEditorUrl: string;
    contentsListUrl: string;
    validateOnPublish: boolean;
    searchFields: string[];
    tags: string[];
};

const EDIT_SCHEMA_DEFAULTS: EditSchemaFormModel = {
    label: '',
    hints: '',
    contentsSidebarUrl: '',
    contentSidebarUrl: '',
    contentEditorUrl: '',
    contentsListUrl: '',
    validateOnPublish: false,
    searchFields: [],
    tags: [],
};

export class EditSchemaForm extends SignalForm<EditSchemaFormModel, UpdateSchemaDto, SchemaPropertiesDto> {
    constructor() {
        super(EDIT_SCHEMA_DEFAULTS, path => {
            maxLength(path.label, 100);
            maxLength(path.hints, 1000);
            maxLength(path.searchFields, 4);
        });
    }

    protected transformLoad(value: Partial<SchemaPropertiesDto>): EditSchemaFormModel {
        return loadWithDefaults(value, EDIT_SCHEMA_DEFAULTS);
    }

    protected transformSubmit(value: EditSchemaFormModel) {
        return new UpdateSchemaDto(submitWithoutEmpty(value));
    }
}

type AddFieldFormModel = { type: FieldType; name: string; isLocalizable: boolean };

export class AddFieldForm extends SignalForm<AddFieldFormModel, AddFieldDto, Partial<AddFieldDto>> {
    public readonly isContentField = computed(() => {
        return this.model().type !== 'UI';
    });

    constructor() {
        super({ type: 'String', name: '', isLocalizable: false }, path => {
            maxLength(path.name, 40);
            SignalValidators.patternWithMessage(path.name, '[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'i18n:schemas.field.nameValidationMessage');
            required(path.type);
            required(path.name);
        });
    }

    protected transformLoad(value: Partial<AddFieldDto>): AddFieldFormModel {
        const { name, properties, partitioning } = value;

        const isLocalizable = partitioning === 'language';

        const type =
            properties ?
            properties.fieldType :
            'String';

        return { name: name ?? '', isLocalizable, type: type as FieldType };
    }

    protected transformSubmit(value: AddFieldFormModel) {
        const { name, type, isLocalizable } = value;

        const properties = createProperties(type);
        const partitioning = isLocalizable ? 'language' : 'invariant';

        return new AddFieldDto({ name, partitioning, properties });
    }
}
