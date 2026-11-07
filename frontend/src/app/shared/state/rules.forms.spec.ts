/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { ContentChangedRuleTriggerDto, RuleElementDto, RuleElementPropertyDto, UsageRuleTriggerDto } from '../model';
import { ConditionTriggerForm, ContentChangedTriggerForm, createTriggerForm, CronJobTriggerForm, StepForm, UsageTriggerForm } from './rules.forms';

describe('StepForm', () => {
    let form: StepForm;

    beforeEach(() => {
        const definition = new RuleElementDto({
            display: 'Step',
            description: 'Step',
            properties: [
                createProperty('url', 'Text', true),
                createProperty('method', 'Dropdown', true, ['GET', 'POST']),
                createProperty('flag', 'Checkbox', true),
                createProperty('hidden', 'None', false),
                createProperty('branches', 'Branches', false),
            ],
        });

        form = TestBed.runInInjectionContext(() => new StepForm(definition, 'Webhook'));
    });

    it('should create model from definition', () => {
        expect(form.model()).toEqual({ url: null, method: 'GET', flag: null, hidden: null, branches: [] });
    });

    it('should only provide editable properties', () => {
        expect(form.editableProperties.map(x => x.name)).toEqual(['url', 'method', 'flag', 'branches']);
    });

    it('should validate required properties', () => {
        const result = form.submit();

        expect(result).toBeNull();
        expect(form.field('url')().errors().map(x => x.kind)).toEqual(['required']);
        expect(form.checkboxField('flag')().errors()).toEqual([]);
    });

    it('should load branches', () => {
        form.load({ url: 'https://squidex.io', branches: [{ condition: null as any, nextStepId: '1' }] });

        expect(form.model()['branches']).toMatchObject([{ condition: '', nextStepId: '1' }]);
    });

    it('should add and remove branches', () => {
        form.addBranch('branches');
        form.addBranch('branches');
        form.branches('branches')[1].condition().value.set('x > 1');
        form.removeBranch('branches', 0);

        expect(form.model()['branches']).toMatchObject([{ condition: 'x > 1' }]);
    });

    it('should validate branch condition length', () => {
        form.addBranch('branches');
        form.branches('branches')[0].condition().value.set('a'.repeat(1001));

        expect(form.branches('branches')[0].condition().errors().map(x => x.kind)).toEqual(['maxLength']);
    });

    it('should add step type when submitting', () => {
        form.load({ url: 'https://squidex.io', flag: false, hidden: 'value', stepType: 'Other' });

        const result = form.submit();

        expect(result).toEqual({ url: 'https://squidex.io', method: 'GET', flag: false, hidden: 'value', branches: [], stepType: 'Webhook' });
    });
});

describe('TriggerForm', () => {
    it('should create form by trigger type', () => {
        TestBed.runInInjectionContext(() => {
            expect(createTriggerForm('AssetChanged')).toBeInstanceOf(ConditionTriggerForm);
            expect(createTriggerForm('Comment')).toBeInstanceOf(ConditionTriggerForm);
            expect(createTriggerForm('SchemaChanged')).toBeInstanceOf(ConditionTriggerForm);
            expect(createTriggerForm('ContentChanged')).toBeInstanceOf(ContentChangedTriggerForm);
            expect(createTriggerForm('CronJob')).toBeInstanceOf(CronJobTriggerForm);
            expect(createTriggerForm('Usage')).toBeInstanceOf(UsageTriggerForm);
            expect(createTriggerForm('Unknown')).toBeUndefined();
        });
    });

    it('should submit condition trigger with trigger type', () => {
        const form = TestBed.runInInjectionContext(() => new ConditionTriggerForm('Comment'));

        form.load({ condition: 'event.text' });

        const result = form.submit();

        expect(result?.triggerType).toEqual('Comment');
        expect((result as any).condition).toEqual('event.text');
    });

    it('should validate usage trigger', () => {
        const form = TestBed.runInInjectionContext(() => new UsageTriggerForm());

        form.form.numDays().value.set(31);

        const result = form.submit();

        expect(result).toBeNull();
        expect(form.form.numDays().errors().map(x => x.kind)).toEqual(['between']);
    });

    it('should submit usage trigger', () => {
        const form = TestBed.runInInjectionContext(() => new UsageTriggerForm());

        const result = form.submit();

        expect(result).toBeInstanceOf(UsageRuleTriggerDto);
        expect((result as UsageRuleTriggerDto).limit).toEqual(20000);
        expect((result as UsageRuleTriggerDto).numDays).toEqual(3);
    });

    it('should require cron expression', () => {
        const form = TestBed.runInInjectionContext(() => new CronJobTriggerForm());

        const result = form.submit();

        expect(result).toBeNull();
        expect(form.form.cronExpression().errors().map(x => x.kind)).toEqual(['required']);
    });

    it('should load content changed trigger', () => {
        const form = TestBed.runInInjectionContext(() => new ContentChangedTriggerForm());

        form.load({ handleAll: true, schemas: [{ schemaId: '1' } as any] });

        expect(form.model()).toMatchObject({ handleAll: true, schemas: [{ schemaId: '1', condition: '' }], referencedSchemas: [] });
    });

    it('should add and remove schemas', () => {
        const form = TestBed.runInInjectionContext(() => new ContentChangedTriggerForm());

        form.addSchema();
        form.addSchema();
        form.addReferencedSchema();
        form.form.schemas[1].schemaId().value.set('2');
        form.removeSchema(0);

        expect(form.model()).toMatchObject({ handleAll: false, schemas: [{ schemaId: '2', condition: '' }], referencedSchemas: [{ schemaId: '', condition: '' }] });
    });

    it('should require schema id', () => {
        const form = TestBed.runInInjectionContext(() => new ContentChangedTriggerForm());

        form.addReferencedSchema();

        const result = form.submit();

        expect(result).toBeNull();
        expect(form.form.referencedSchemas[0].schemaId().errors().map(x => x.kind)).toEqual(['required']);
    });

    it('should submit content changed trigger', () => {
        const form = TestBed.runInInjectionContext(() => new ContentChangedTriggerForm());

        form.load({ handleAll: false, schemas: [{ schemaId: '1', condition: 'true' }] });

        const result = form.submit();

        expect(result).toBeInstanceOf(ContentChangedRuleTriggerDto);
        expect((result as ContentChangedRuleTriggerDto).schemas?.[0].schemaId).toEqual('1');
    });
});

function createProperty(name: string, editor: string, isRequired: boolean, options?: string[]) {
    return new RuleElementPropertyDto({ name, editor, display: name, isRequired, isFormattable: false, options });
}
