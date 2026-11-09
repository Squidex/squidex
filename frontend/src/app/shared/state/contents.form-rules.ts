/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-implied-eval */


import { Types } from '@app/framework';
import { FieldDto, FieldRuleDto, NestedFieldDto, SchemaDto } from './../model';

export type RuleContext = { data: any; user?: any };
export type RuleAction = FieldRuleDto['action'];

const EMPTY_RULES: ReadonlyArray<CompiledRule> = [];
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

type RuleScope = { schema: SchemaDto; start: number };

export class ContentRules {
    private readonly compiledBySchema: { [id: string]: ReadonlyArray<CompiledRule> } = {};
    private readonly rulesByPath = new Map<string, ReadonlyArray<CompiledRule>>();

    constructor(
        private readonly schema: SchemaDto,
        private readonly schemas: { [id: string]: SchemaDto },
    ) {
    }

    public getRules(keys: ReadonlyArray<string>, root: any, field: FieldDto | NestedFieldDto): ReadonlyArray<CompiledRule> {
        const scopes = getScopes(keys, root, this.schemas);

        const cacheKey = `${field.fieldId}|${keys.filter(x => !isIndex(x)).join('.')}|${scopes.map(x => `${x.schema.id}@${x.start}`).join(',')}`;

        let result = this.rulesByPath.get(cacheKey);
        if (!result) {
            result = this.findRules(this.schema, keys, field);

            for (const scope of scopes) {
                result = fastMerge(result, this.findRules(scope.schema, keys.slice(scope.start), field));
            }

            this.rulesByPath.set(cacheKey, result);
        }

        return result;
    }

    private findRules(schema: SchemaDto, keys: ReadonlyArray<string>, field: FieldDto | NestedFieldDto) {
        const rules = this.compileRules(schema);
        if (rules.length === 0) {
            return EMPTY_RULES;
        }

        const pathNormal = keys.filter(x => !isIndex(x)).join('.');
        const pathSimple = getSimplePath(pathNormal);

        return rules.filter(x => x.isApplied(pathNormal, field) || x.isApplied(pathSimple, field));
    }

    private compileRules(schema: SchemaDto) {
        let result = this.compiledBySchema[schema.id];
        if (!result) {
            result = schema.fieldRules.map(x => new CompiledRule(x, true));

            this.compiledBySchema[schema.id] = result;
        }

        return result;
    }
}

function getScopes(keys: ReadonlyArray<string>, root: any, schemas: { [id: string]: SchemaDto }) {
    const result: RuleScope[] = [];

    let current = root;

    for (let i = 0; i < keys.length - 1; i++) {
        current = current?.[keys[i]];

        const schema = schemas[current?.schemaId];
        if (schema) {
            result.push({ schema, start: i + 1 });
        }
    }

    return result;
}

function isIndex(key: string) {
    return /^\d+$/.test(key);
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
