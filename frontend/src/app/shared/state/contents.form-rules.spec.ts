/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { createProperties, FieldRuleDto } from '@app/shared/internal';
import { TestValues } from './_test-helpers';
import { ContentRules } from './contents.form-rules';

const { createField, createSchema } = TestValues;

describe('ContentRules', () => {
    const field = createField({ id: 1, properties: createProperties('String') });

    const componentSchema = createSchema({
        id: 2,
        fieldRules: [
            new FieldRuleDto({ field: 'field1', action: 'Disable', condition: 'data.field1 > 1' }),
        ],
    });

    const rootSchema = createSchema({ id: 1 });

    const keys = ['component', 'iv', 'field1'];

    it('should return relative rules of the component schema', () => {
        const contentRules = new ContentRules(rootSchema, { [componentSchema.id]: componentSchema });

        const rules = contentRules.getRules(keys, createData(componentSchema.id), field);

        expect(rules.length).toEqual(1);
        expect(rules[0].action).toEqual('Disable');
    });

    it('should not recompute rules if schema has not changed', () => {
        const contentRules = new ContentRules(rootSchema, { [componentSchema.id]: componentSchema });

        const rules1 = contentRules.getRules(keys, createData(componentSchema.id), field);
        const rules2 = contentRules.getRules(keys, createData(componentSchema.id), field);

        expect(rules1).toBe(rules2);
    });

    it('should recompute rules if schema has changed', () => {
        const contentRules = new ContentRules(rootSchema, { [componentSchema.id]: componentSchema });

        const rules1 = contentRules.getRules(keys, createData('unknown'), field);
        const rules2 = contentRules.getRules(keys, createData(componentSchema.id), field);

        expect(rules1.length).toEqual(0);
        expect(rules2.length).toEqual(1);
    });

    function createData(schemaId: string) {
        return { component: { iv: { schemaId, field1: 2 } } };
    }
});
