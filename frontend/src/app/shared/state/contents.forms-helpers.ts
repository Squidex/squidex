/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Signal, signal, WritableSignal } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { AppLanguageDto, FieldDto, fieldInvariant, isValidValue, Language, NestedFieldDto, SchemaDto } from '../internal';
import type { ContentValues } from './contents.forms-values';

export type TranslationStatuses = { [language: string]: number };

export type AnyFieldDto = NestedFieldDto;

export function contentsTranslationStatus(datas: any[], schema: SchemaDto, languages: ReadonlyArray<Language>) {
    const result: TranslationStatuses = {};

    for (const data of datas) {
        const status = contentTranslationStatus(data, schema, languages);

        for (const language of languages) {
            const iso2Code = language.iso2Code;

            result[iso2Code] = (result[iso2Code] || 0) + status[iso2Code];
        }
    }

    for (const language of languages) {
        const iso2Code = language.iso2Code;

        result[iso2Code] = Math.round(result[iso2Code] / datas.length);
    }

    return result;
}

export function contentTranslationStatus(data: any, schema: SchemaDto, languages: ReadonlyArray<Language>) {
    const result: TranslationStatuses = {};

    const localizedFields = schema.fields.filter(x => x.isLocalizable);

    for (const language of languages) {
        let percent = 0;

        for (const field of localizedFields) {
            if (isValidValue(data?.[field.name]?.[language.iso2Code])) {
                percent++;
            }
        }

        if (localizedFields.length > 0) {
            percent = Math.round(100 * percent / localizedFields.length);
        } else {
            percent = 100;
        }

        result[language.iso2Code] = percent;
    }

    return result;
}

export function fieldTranslationStatus(data: any) {
    const result: { [field: string]: boolean } = {};

    for (const [key, value] of Object.entries(data)) {
        result[key] = isValidValue(value);
    }

    return result;
}

export type FieldGroup<T = FieldDto> = { separator?: T; fields: T[]; id: string };

export function groupFields<T extends AnyFieldDto>(fields: ReadonlyArray<T>, keepEmpty = false): FieldGroup<T>[] {
    const result: FieldGroup<T>[] = [];

    let currentSeparator: T | undefined;
    let currentFields: T[] = [];

    const addGroup = () => {
        if (currentFields.length > 0 || keepEmpty) {
            const id = currentSeparator?.fieldId.toString() || 'DEFAULT';

            result.push({ separator: currentSeparator, fields: currentFields, id });
        }
    };

    for (const field of fields) {
        if (field.properties.isContentField) {
            currentFields.push(field);
        } else {
            addGroup();
            currentFields = [];
            currentSeparator = field;
        }
    }

    addGroup();
    return result;
}

export class FieldSection<TSeparator, TChild extends { hidden: Signal<boolean> }> {
    public readonly hidden = computed(() => {
        return this.fields.every(x => x.hidden());
    });

    public readonly visible = computed(() => {
        return !this.hidden();
    });

    constructor(
        public readonly separator: TSeparator | undefined,
        public readonly fields: ReadonlyArray<TChild>,
    ) {
    }
}

type Partition = { key: string; isOptional: boolean };

export class PartitionConfig {
    private readonly invariant: ReadonlyArray<Partition> = [{ key: fieldInvariant, isOptional: false }];
    private readonly languages: ReadonlyArray<Partition>;

    constructor(languages: ReadonlyArray<AppLanguageDto>) {
        this.languages = languages.map(l => this.get(l));
    }

    public get(language?: AppLanguageDto) {
        if (!language) {
            return this.invariant[0];
        }

        return { key: language.iso2Code, isOptional: language.isOptional };
    }

    public getAll(field: FieldDto) {
        return field.isLocalizable ? this.languages : this.invariant;
    }

    public isOptional(key: string | undefined) {
        return this.languages.find(x => x.key === key)?.isOptional === true;
    }
}

export type FormGlobals = Readonly<{
    partitions: PartitionConfig;
    schema: SchemaDto;
    schemas: { [id: string ]: SchemaDto };
    values: ContentValues;
}>;

export type ControlArgs<T = AnyFieldDto> = Readonly<{
    field: T;
    globals: FormGlobals;
    isOptional: boolean;
    partition: string;
    path: string;
}>;

export abstract class AbstractContentForm<T extends AnyFieldDto> {
    private readonly collapsedState = signal<boolean | null>(null);

    public readonly field: T;
    public readonly globals: FormGlobals;
    public readonly isOptional: boolean;
    public readonly partition: string;
    public readonly path: string;

    public readonly collapsed = this.collapsedState.asReadonly();

    public readonly disabled = computed(() => {
        return this.form().disabled();
    });

    public readonly hidden = computed(() => {
        return this.form().hidden();
    });

    public readonly visible = computed(() => {
        return !this.hidden();
    });

    public readonly invalid = computed(() => {
        return this.form().invalid();
    });

    public readonly value = computed<any>(() => {
        return this.formValue();
    });

    protected get formValue() {
        return this.form().value as WritableSignal<any>;
    }

    constructor(
        public readonly args: ControlArgs<T>,
        public readonly form: FieldTree<any>,
    ) {
        this.field = args.field;
        this.globals = args.globals;
        this.isOptional = args.isOptional;
        this.partition = args.partition;
        this.path = args.path;
    }

    public relativePath(relative: string) {
        return `${this.path}.${relative}`;
    }

    public setValue(value: any) {
        this.formValue.set(this.globals.values.createValue(this.field, value, this.partition, false));
        this.form().reset();
    }

    public unset() {
        this.formValue.set(null);
    }

    public collapse() {
        this.collapsedState.set(true);
    }

    public expand() {
        this.collapsedState.set(false);
    }
}
