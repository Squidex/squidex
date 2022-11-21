/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-implied-eval */
/* eslint-disable no-useless-return */

import { AbstractControl, ValidatorFn } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { isValidValue, Language } from './../internal';
import { AppLanguageDto } from './../services/app-languages.service';
import { FieldDto, RootFieldDto, SchemaDto } from './../services/schemas.service';
import { fieldInvariant } from './../services/schemas.types';
import { CompiledRules, RuleContext, RulesProvider } from './contents.form-rules';

export type TranslationStatus = { [language: string]: number };

export function contentsTranslationStatus(datas: any[], schema: SchemaDto, languages: ReadonlyArray<Language>) {
    const result: TranslationStatus = {};

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
    const result: TranslationStatus = {};

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
    private readonly hidden$ = new BehaviorSubject<boolean>(false);

    public get hidden() {
        return this.hidden$.value;
    }

    public get hiddenChanges(): Observable<boolean> {
        return this.hidden$;
    }

    public get visibleChanges(): Observable<boolean> {
        return this.hidden$.pipe(map(x => !x));
    }

    protected setHidden(hidden: boolean) {
        if (hidden !== this.hidden) {
            this.hidden$.next(hidden);
        }
    }
}

export type FieldGroup<T = FieldDto> = { separator?: T; fields: T[] };

export function groupFields<T extends FieldDto>(fields: ReadonlyArray<T>, keepEmpty = false): FieldGroup<T>[] {
    const result: FieldGroup<T>[] = [];

    let currentSeparator: T | undefined;
    let currentFields: T[] = [];

    for (const field of fields) {
        if (field.properties.isContentField) {
            currentFields.push(field);
        } else {
            if (currentFields.length > 0 || keepEmpty) {
                result.push({ separator: currentSeparator, fields: currentFields });
            }

            currentFields = [];
            currentSeparator = field;
        }
    }

    if (currentFields.length > 0 || keepEmpty) {
        result.push({ separator: currentSeparator, fields: currentFields });
    }

    return result;
}

export class FieldSection<TSeparator, TChild extends { hidden: boolean }> extends Hidden {
    constructor(
        public readonly separator: TSeparator | undefined,
        public readonly fields: ReadonlyArray<TChild>,
    ) {
        super();
    }

    public updateHidden() {
        let visible = false;

        for (const child of this.fields) {
            visible = visible || !child.hidden;
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

    public getAll(field: RootFieldDto) {
        return field.isLocalizable ? this.languages : this.invariant;
    }
}

export type AbstractContentFormState = {
    isDisabled?: boolean;
    isHidden?: boolean;
    isRequired?: boolean;
};

export interface FormGlobals {
    partitions: PartitionConfig;
    schema: SchemaDto;
    schemas: { [id: string ]: SchemaDto };
    remoteValidator?: ValidatorFn;
}

export abstract class AbstractContentForm<T extends FieldDto, TForm extends AbstractControl> extends Hidden {
    private readonly collapsed$ = new BehaviorSubject<boolean | null>(null);
    private readonly ruleSet: CompiledRules;

    public get collapsed() {
        return this.collapsed$.value;
    }

    public get collapsedChanges(): Observable<boolean | null> {
        return this.collapsed$;
    }

    protected constructor(
        public readonly globals: FormGlobals,
        public readonly field: T,
        public readonly fieldPath: string,
        public readonly form: TForm,
        public readonly isOptional: boolean,
        public readonly rules: RulesProvider,
    ) {
        super();

        this.ruleSet = rules.getRules(this);
    }

    public path(relative: string) {
        return `${this.fieldPath}.${relative}`;
    }

    public collapse() {
        this.collapsed$.next(true);
    }

    public expand() {
        this.collapsed$.next(false);
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

    public setValue(value: any) {
        this.form.reset(value);
    }

    public unset() {
        this.form.setValue(undefined);
    }

    protected updateCustomState(_context: RuleContext, _itemData: any, _state: AbstractContentFormState): void {
        return;
    }
}

const SELF = { onlySelf: true };
