/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-implied-eval */
/* eslint-disable no-useless-return */

import { AbstractControl, ValidatorFn } from '@angular/forms';
import { Types } from '@app/framework';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppLanguageDto } from './../services/app-languages.service';
import { FieldDto, FieldRule, RootFieldDto, SchemaDto } from './../services/schemas.service';
import { fieldInvariant } from './../services/schemas.types';

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

export function groupFields<T extends FieldDto>(fields: ReadonlyArray<T>): { separator?: T; fields: ReadonlyArray<T> }[] {
    const result: { separator?: T; fields: ReadonlyArray<T> }[] = [];

    let currentSeparator: T | undefined;
    let currentFields: T[] = [];

    for (const field of fields) {
        if (field.properties.isContentField) {
            currentFields.push(field);
        } else {
            if (currentFields.length > 0) {
                result.push({ separator: currentSeparator, fields: currentFields });
            }

            currentFields = [];
            currentSeparator = field;
        }
    }

    if (currentFields.length > 0) {
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

type RuleContext = { data: any; user?: any };

export class CompiledRule {
    private readonly function: Function;

    public get field() {
        return this.rule.field;
    }

    public get action() {
        return this.rule.action;
    }

    constructor(
        private readonly rule: FieldRule,
        private readonly useItemData: boolean,
    ) {
        try {
            this.function = new Function(`return function(user, ctx, data, itemData) { return ${rule.condition} }`)();
        } catch {
            this.function = () => false;
        }
    }

    public eval(context: RuleContext, itemData: any) {
        try {
            const data = this.useItemData ? itemData || context.data : context.data;

            return this.function(context.user, context, data, itemData);
        } catch {
            return false;
        }
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

const EMPTY_RULES: CompiledRule[] = [];

export interface RulesProvider {
    compileRules(schema: SchemaDto | undefined): ReadonlyArray<CompiledRule>;

    setSchema(schema?: SchemaDto): void;

    getRules(form: AbstractContentForm<any, any>): ReadonlyArray<CompiledRule>;
}

export class ComponentRulesProvider implements RulesProvider {
    private schema?: SchemaDto;

    constructor(
        private readonly parentPath: string,
        private readonly parent: RulesProvider,
    ) {
    }

    public setSchema(schema?: SchemaDto) {
        this.schema = schema;
    }

    public compileRules(schema: SchemaDto | undefined): ReadonlyArray<CompiledRule> {
        return this.parent.compileRules(schema);
    }

    public getRules(form: AbstractContentForm<any, any>) {
        return Types.fastMerge(this.parent.getRules(form), this.getRelativeRules(form));
    }

    private getRelativeRules(form: AbstractContentForm<any, any>) {
        const rules = this.compileRules(this.schema);

        if (rules.length === 0) {
            return EMPTY_RULES;
        }

        const pathField = form.fieldPath.substr(this.parentPath.length + 1);
        const pathSimplified = pathField.replace('.iv.', '.');

        return rules.filter(x => x.field === pathField || x.field === pathSimplified);
    }
}

export class RootRulesProvider implements RulesProvider {
    private readonly compiledRules: { [id: string]: ReadonlyArray<CompiledRule> } = {};
    private readonly rules: ReadonlyArray<CompiledRule>;

    constructor(schema: SchemaDto) {
        this.rules = schema.fieldRules.map(x => new CompiledRule(x, false));
    }

    public setSchema() {
        return;
    }

    public compileRules(schema: SchemaDto | undefined) {
        if (!schema) {
            return EMPTY_RULES;
        }

        let result = this.compileRules[schema.id];

        if (!result) {
            result = schema.fieldRules.map(x => new CompiledRule(x, true));

            this.compiledRules[schema.id] = result;
        }

        return result;
    }

    public getRules(form: AbstractContentForm<any, any>) {
        const rules = this.rules;

        if (rules.length === 0) {
            return EMPTY_RULES;
        }

        const pathField = form.fieldPath;
        const pathSimplified = pathField.replace('.iv.', '.');

        return rules.filter(x => x.field === pathField || x.field === pathSimplified);
    }
}

export abstract class AbstractContentForm<T extends FieldDto, TForm extends AbstractControl> extends Hidden {
    private readonly disabled$ = new BehaviorSubject<boolean>(false);
    private readonly currentRules: ReadonlyArray<CompiledRule>;

    public get disabled() {
        return this.disabled$.value;
    }

    public get disabledChanges(): Observable<boolean> {
        return this.disabled$;
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

        this.currentRules = rules.getRules(this);
    }

    public path(relative: string) {
        return `${this.fieldPath}.${relative}`;
    }

    public updateState(context: RuleContext, fieldData: any, itemData: any, parentState: AbstractContentFormState) {
        const state = {
            isDisabled: this.field.isDisabled || parentState.isDisabled === true,
            isHidden: parentState.isHidden === true,
            isRequired: this.field.properties.isRequired && !this.isOptional,
        };

        for (const rule of this.currentRules) {
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

        this.updateCustomState(context, fieldData, itemData, state);
    }

    public unset() {
        this.form.setValue(undefined);
    }

    protected updateCustomState(_context: RuleContext, _fieldData: any, _itemData: any, _state: AbstractContentFormState): void {
        return;
    }

    public prepareLoad(_data: any): void {
        return;
    }
}

const SELF = { onlySelf: true };
