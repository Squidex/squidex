/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-return */

import { AbstractControl, ValidatorFn } from '@angular/forms';
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

type RuleContext = { data: any; itemData?: any; user?: any };

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
    ) {
        try {
            // eslint-disable-next-line @typescript-eslint/no-implied-eval
            this.function = new Function(`return function(user, ctx, data, itemData) { return ${rule.condition} }`)();
        } catch {
            this.function = () => false;
        }
    }

    public eval(context: RuleContext) {
        try {
            return this.function(context.user, context, context.data, context.itemData);
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
    allRules: ReadonlyArray<CompiledRule>;
    partitions: PartitionConfig;
    schema: SchemaDto;
    schemas: { [id: string ]: SchemaDto };
    remoteValidator?: ValidatorFn;
}

export abstract class AbstractContentForm<T extends FieldDto, TForm extends AbstractControl> extends Hidden {
    private readonly disabled$ = new BehaviorSubject<boolean>(false);
    private readonly rules: ReadonlyArray<CompiledRule>;

    public get disabled() {
        return this.disabled$.value;
    }

    public get disabledChanges(): Observable<boolean> {
        return this.disabled$;
    }

    protected constructor(
        public readonly globals: FormGlobals,
        public readonly fieldPath: string,
        public readonly field: T,
        public readonly form: TForm,
        public readonly isOptional: boolean,
    ) {
        super();

        const simplifiedPath = fieldPath.replace('.iv.', '.');

        this.rules = globals.allRules.filter(x => x.field === fieldPath || x.field === simplifiedPath);
    }

    public updateState(context: RuleContext, parentState: AbstractContentFormState) {
        const state = {
            isDisabled: this.field.isDisabled || parentState.isDisabled === true,
            isHidden: parentState.isHidden === true,
            isRequired: this.field.properties.isRequired && !this.isOptional,
        };

        if (this.rules) {
            for (const rule of this.rules) {
                if (rule.eval(context)) {
                    if (rule.action === 'Disable') {
                        state.isDisabled = true;
                    } else if (rule.action === 'Hide') {
                        state.isHidden = true;
                    } else {
                        state.isRequired = true;
                    }
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

        this.updateCustomState(context, state);
    }

    public unset() {
        this.form.setValue(undefined);
    }

    protected updateCustomState(_context: RuleContext, _state: AbstractContentFormState): void {
        return;
    }

    public prepareLoad(_data: any): void {
        return;
    }
}

const SELF = { onlySelf: true };
