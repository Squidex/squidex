/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-implied-eval */
/* eslint-disable no-useless-return */

import { Types } from '@app/framework';
import { FieldRuleDto, SchemaDto } from './../model';

export type RuleContext = { data: any; user?: any };
export type RuleForm = { fieldPath: string };

export interface CompiledRules {
    get rules(): ReadonlyArray<CompiledRule>;
}

export interface RulesProvider {
    compileRules(schema: SchemaDto): ReadonlyArray<CompiledRule>;

    getRules(form: RuleForm): CompiledRules;
}

export class CompiledRule {
    private readonly function: Function;

    public get field() {
        return this.rule.field;
    }

    public get action() {
        return this.rule.action;
    }

    constructor(
        private readonly rule: FieldRuleDto,
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
const EMPTY_RULES: CompiledRule[] = [];
const EMPTY_RULES_STATIC = { rules: EMPTY_RULES };

class ComponentRules implements ComponentRules {
    private previouSchema?: SchemaDto;
    private compiledRules: ReadonlyArray<CompiledRule> = [];

    public get rules() {
        const schema = this.schema();

        if (schema !== this.previouSchema) {
            if (schema) {
                this.compiledRules = Types.fastMerge(this.parentRules.getRules(this.form).rules, this.getRelativeRules(this.form, schema));
            } else {
                this.compiledRules = EMPTY_RULES;
            }
        }

        return this.compiledRules;
    }

    constructor(
        private readonly form: RuleForm,
        private readonly parentPath: string,
        private readonly parentRules: RulesProvider,
        private readonly schema: () => SchemaDto | undefined,
    ) {
    }

    private getRelativeRules(form: RuleForm, schema: SchemaDto) {
        const rules = this.parentRules.compileRules(schema);

        if (rules.length === 0) {
            return EMPTY_RULES;
        }

        const pathField = form.fieldPath.substring(this.parentPath.length + 1);
        const pathSimple = getSimplePath(pathField);

        return rules.filter(x => x.field === pathField || x.field === pathSimple);
    }
}

export class ComponentRulesProvider implements RulesProvider {
    constructor(
        private readonly parentPath: string,
        private readonly parentRules: RulesProvider,
        private readonly schema: () => SchemaDto | undefined,
    ) {
    }

    public compileRules(schema: SchemaDto) {
        return this.parentRules.compileRules(schema);
    }

    public getRules(form: RuleForm) {
        return new ComponentRules(form, this.parentPath, this.parentRules, this.schema);
    }
}

export class RootRulesProvider implements RulesProvider {
    private readonly rulesCache: { [id: string]: ReadonlyArray<CompiledRule> } = {};
    private readonly rules: ReadonlyArray<CompiledRule>;

    constructor(schema: SchemaDto) {
        this.rules = this.compileRules(schema);
    }

    public compileRules(schema: SchemaDto) {
        if (!schema) {
            return EMPTY_RULES;
        }

        let result = this.rulesCache[schema.id];

        if (!result) {
            result = schema.fieldRules.map(x => new CompiledRule(x, true));

            this.rulesCache[schema.id] = result;
        }

        return result;
    }

    public getRules(form: RuleForm) {
        const allRules = this.rules;

        if (allRules.length === 0) {
            return EMPTY_RULES_STATIC;
        }

        const pathField = form.fieldPath;
        const pathSimple = getSimplePath(pathField);

        const rules = allRules.filter(x => x.field === pathField || x.field === pathSimple);

        return { rules };
    }
}

function getSimplePath(path: string) {
    const parts = path.split('.');

    if (parts.length >= 2) {
        parts.splice(1, 1);
    }

    return parts.join('.');
}