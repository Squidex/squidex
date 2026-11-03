/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { createProperties, FieldRuleDto, SchemaDto } from '@app/shared/internal';
import { TestValues } from './_test-helpers';
import { ComponentRulesProvider, RootRulesProvider, RuleForm } from './contents.form-rules';

const { createField, createSchema } = TestValues;

describe('ComponentRulesProvider', () => {
    const field = createField({ id: 1, properties: createProperties('String') });

    const componentSchema = createSchema({
        id: 2,
        fieldRules: [
            new FieldRuleDto({ field: 'field1', action: 'Disable', condition: 'data.field1 > 1' }),
        ],
    });

    const rootSchema = createSchema({ id: 1 });

    it('should return relative rules of the component schema', () => {
        const form: RuleForm = { path: 'component.field1', field };

        const rules = new ComponentRulesProvider('component', new RootRulesProvider(rootSchema), () => componentSchema).getRules(form);

        expect(rules.rules.length).toEqual(1);
        expect(rules.rules[0].action).toEqual('Disable');
    });

    it('should not recompute rules if schema has not changed', () => {
        const form: RuleForm = { path: 'component.field1', field };

        const rootRules = new RootRulesProvider(rootSchema);
        const rootGetRules = rootRules.getRules.bind(rootRules);

        let rootCalls = 0;

        rootRules.getRules = (x: RuleForm) => {
            rootCalls++;
            return rootGetRules(x);
        };

        const rules = new ComponentRulesProvider('component', rootRules, () => componentSchema).getRules(form);

        const rules1 = rules.rules;
        const rules2 = rules.rules;

        expect(rules1).toBe(rules2);
        expect(rootCalls).toEqual(1);
    });

    it('should recompute rules if schema has changed', () => {
        const form: RuleForm = { path: 'component.field1', field };

        let schema: SchemaDto | undefined = undefined;

        const rules = new ComponentRulesProvider('component', new RootRulesProvider(rootSchema), () => schema).getRules(form);

        expect(rules.rules.length).toEqual(0);

        schema = componentSchema;

        expect(rules.rules.length).toEqual(1);
    });
});
