/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject, Observable } from 'rxjs';
import { AppLanguageDto } from './../services/app-languages.service';
import { FieldRule, RootFieldDto } from './../services/schemas.service';
import { fieldInvariant } from './../services/schemas.types';

export abstract class Hidden {
    private readonly hidden$ = new BehaviorSubject<boolean>(false);

    public get hidden() {
        return this.hidden$.value;
    }

    public get hiddenChanges(): Observable<boolean> {
        return this.hidden$;
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
        public readonly fields: ReadonlyArray<TChild>
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

type Partition = { key: string, isOptional: boolean };

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

export class CompiledRule {
    private function: Function;

    public get field() {
        return this.rule.field;
    }

    public get action() {
        return this.rule.action;
    }

    constructor(
        private readonly rule: FieldRule
    ) {
        try {
            this.function = new Function(`return function(user, data, itemData) { return ${rule.condition} }`)();
        } catch {
            this.function = () => false;
        }
    }

    public eval(user: any, data: any, itemData?: any) {
        try {
            return this.function(user, data, itemData);
        } catch {
            return false;
        }
    }
}