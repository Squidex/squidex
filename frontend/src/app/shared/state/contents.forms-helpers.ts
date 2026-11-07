/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Signal, signal } from '@angular/core';
import { AbstractControl, ValidatorFn } from '@angular/forms';
import { AppLanguageDto, FieldDto, fieldInvariant, isValidValue, Language, NestedFieldDto, SchemaDto } from '../internal';
import { CompiledRules, RuleContext, RulesProvider } from './contents.form-rules';

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

export abstract class Hidden {
    private readonly hiddenState = signal(false);

    public readonly hidden = this.hiddenState.asReadonly();

    public readonly visible = computed(() => {
        return !this.hiddenState();
    });

    protected setHidden(hidden: boolean) {
        this.hiddenState.set(hidden);
    }
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

export class FieldSection<TSeparator, TChild extends { hidden: Signal<boolean> }> extends Hidden {
    constructor(
        public readonly separator: TSeparator | undefined,
        public readonly fields: ReadonlyArray<TChild>,
    ) {
        super();
    }

    public updateHidden() {
        let visible = false;

        for (const child of this.fields) {
            visible = visible || !child.hidden();
        }

        this.setHidden(!visible);
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
}

export type AbstractContentFormState = Readonly<{
    isDisabled?: boolean;
    isHidden?: boolean;
    isRequired?: boolean;
}>;

export type FormGlobals = Readonly<{
    partitions: PartitionConfig;
    remoteValidator?: ValidatorFn;
    schema: SchemaDto;
    schemas: { [id: string ]: SchemaDto };
}>;

export type ControlArgs<T = AnyFieldDto> = Readonly<{
    field: T;
    globals: FormGlobals;
    isOptional: boolean;
    partition: string;
    path: string;
    rules: RulesProvider;
}>;

export abstract class AbstractContentForm<T extends AnyFieldDto, TForm extends AbstractControl> extends Hidden {
    private readonly collapsedState = signal<boolean | null>(null);
    private readonly formDisabled = signal(false);
    private readonly formInvalid = signal(false);
    private readonly formValue = signal<any>(undefined);
    private readonly ruleSet: CompiledRules;

    public readonly field: T;
    public readonly globals: FormGlobals;
    public readonly isOptional: boolean;
    public readonly partition: string;
    public readonly path: string;

    public readonly collapsed = this.collapsedState.asReadonly();

    public readonly disabled = this.formDisabled.asReadonly();

    public readonly invalid = this.formInvalid.asReadonly();

    public readonly value = this.formValue.asReadonly();

    protected constructor(
        public readonly args: ControlArgs<T>,
        public readonly form: TForm,
    ) {
        super();
        this.field = args.field;
        this.globals = args.globals;
        this.isOptional = args.isOptional;
        this.partition = args.partition;
        this.path = args.path;
        this.ruleSet = args.rules.getRules(args);

        this.syncFormState();
        this.form.events.subscribe(() => {
            this.syncFormState();
        });
    }

    protected syncFormState() {
        this.formDisabled.set(this.form.disabled);
        this.formInvalid.set(this.form.invalid);
        this.formValue.set(this.form.value);
    }

    public relativePath(relative: string) {
        return `${this.path}.${relative}`;
    }

    public setValue(value: any) {
        this.form.reset(value);
    }

    public unset() {
        this.form.setValue(undefined);
    }

    public collapse() {
        this.collapsedState.set(true);
    }

    public expand() {
        this.collapsedState.set(false);
    }

    public updateState(context: RuleContext, itemData: any, parentState: AbstractContentFormState) {
        const state = {
            isDisabled: this.field.isDisabled || parentState.isDisabled === true,
            isHidden: parentState.isHidden === true,
            isRequired: this.field.properties.isRequired && !this.isOptional,
        };

        for (const rule of this.ruleSet.rules) {
            if (rule.eval(context, itemData)) {
                if (rule.action === 'Disable') {
                    state.isDisabled = true;
                } else if (rule.action === 'Hide') {
                    state.isHidden = true;
                } else {
                    state.isRequired = true;
                }
            }
        }

        this.setHidden(state.isHidden);

        if (state.isDisabled !== this.form.disabled) {
            if (state.isDisabled) {
                this.form.disable(SELF);
            } else {
                this.form.enable(SELF);
            }
        }

        this.updateCustomState(context, itemData, state);
    }

    protected updateCustomState(_context: RuleContext, _itemData: any, _state: AbstractContentFormState): void {}
}

const SELF = { onlySelf: true };
