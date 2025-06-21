/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-implied-eval */
/* eslint-disable no-useless-return */

import { Types } from '@app/framework';
import { FieldDto, FieldRuleDto, NestedFieldDto, SchemaDto } from './../model';

export type RuleContext = { data: any; user?: any };
export type RuleForm = { path: string; field: FieldDto | NestedFieldDto };

export interface CompiledRules {
    get rules(): ReadonlyArray<CompiledRule>;
}

export interface RulesProvider {
    compileRules(schema: SchemaDto): ReadonlyArray<CompiledRule>;

    getRules(form: RuleForm): CompiledRules;
}

const EMPTY_RULES_ARRAY: CompiledRule[] = [];
const EMPTY_RULES_STATIC = { rules: EMPTY_RULES_ARRAY };
const TAG_PREFIX = 'tag:';

export class CompiledRule {
    private readonly function: Function;
    private readonly evaluator: (path: string, field: FieldDto | NestedFieldDto) => boolean;

    public get action() {
        return this.rule.action;
    }

    constructor(
        private readonly rule: FieldRuleDto,
        private readonly useItemData: boolean,
    ) {
        if (rule.field.startsWith(TAG_PREFIX)) {
            const tag = rule.field.substring(TAG_PREFIX.length).trim();

            this.evaluator = (_, field) => {
                const tags = field.properties?.tags;

                return Types.isArray(tags) && tags.indexOf(tag) >= 0;
            };
        } else {
            this.evaluator = (path, _) => {
                return this.rule.field === path;
            };
        }

        if (!rule.condition) {
            this.function = () => true;
        } else {
            try {
                this.function = new Function(`return function(user, ctx, data, itemData) { return ${rule.condition} }`)();
            } catch {
                this.function = () => false;
            }
        }
    }

    public isApplied(path: string, field: FieldDto | NestedFieldDto) {
        return this.evaluator(path, field);
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

class ComponentRules implements CompiledRules {
    private previouSchema?: SchemaDto;
    private compiledRules: ReadonlyArray<CompiledRule> = [];

    public get rules() {
        const schema = this.schema();

        if (schema !== this.previouSchema) {
            if (schema) {
                this.compiledRules = fastMerge(this.parentRules.getRules(this.form).rules, this.getRelativeRules(this.form, schema));
            } else {
                this.compiledRules = EMPTY_RULES_ARRAY;
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
            return EMPTY_RULES_ARRAY;
        }

        const pathNormal = form.path.substring(this.parentPath.length + 1);
        const pathSimple = getSimplePath(pathNormal);

        return rules.filter(x => x.isApplied(pathNormal, form.field) || x.isApplied(pathSimple, form.field));
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
    private readonly rulesRoot: ReadonlyArray<CompiledRule>;

    constructor(schema: SchemaDto) {
        this.rulesRoot = this.compileRules(schema);
    }

    public compileRules(schema: SchemaDto) {
        if (!schema) {
            return EMPTY_RULES_ARRAY;
        }

        let result = this.rulesCache[schema.id];

        if (!result) {
            result = schema.fieldRules.map(x => new CompiledRule(x, true));

            this.rulesCache[schema.id] = result;
        }

        return result;
    }

    public getRules(form: RuleForm) {
        const allRules = this.rulesRoot;

        if (allRules.length === 0) {
            return EMPTY_RULES_STATIC;
        }

        const pathNormal = form.path;
        const pathSimple = getSimplePath(pathNormal);

        const rules = allRules.filter(x => x.isApplied(pathNormal, form.field) || x.isApplied(pathSimple, form.field));

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

function fastMerge<T>(lhs: ReadonlyArray<T>, rhs: ReadonlyArray<T>) {
    if (rhs.length === 0) {
        return lhs;
    }

    if (lhs.length === 0) {
        return rhs;
    }

    return [...lhs, ...rhs];
}