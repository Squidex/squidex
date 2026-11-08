/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FieldTree, FormField } from '@angular/forms/signals';
import { createProperties, FieldRuleDto, SchemaDto } from '../model';
import { CreateIndexForm } from './indexes.forms';
import { DateTimeFieldFormModel, EditFieldForm, NumberFieldFormModel, StringFieldFormModel } from './schemas-fields.forms';
import { AddFieldForm, ConfigureFieldRulesForm, ConfigurePreviewUrlsForm, CreateSchemaForm, EditSchemaForm, EditSchemaScriptsForm } from './schemas.forms';

describe('EditFieldForm', () => {
    it('should create model with type specific defaults', () => {
        const form = createEditForm(createProperties('Number'));

        const model = form.model() as NumberFieldFormModel;
        expect(model.label).toEqual('');
        expect(model.searchMode).toEqual('Default');
        expect(model.isUnique).toBe(false);
        expect(model.maxValue).toBeNaN();
        expect(model.allowedValues).toBeNull();
    });

    it('should replace null and undefined values with defaults on load', () => {
        const properties = createProperties('String', {
            label: 'Label',
            pattern: null,
            maxLength: 10,
        });

        const form = createEditForm(properties);
        form.load(properties);

        const model = form.model() as StringFieldFormModel;
        expect(model.label).toEqual('Label');
        expect(model.pattern).toEqual('');
        expect(model.maxLength).toEqual(10);
        expect(model.minLength).toBeNaN();
    });

    it('should convert empty values to undefined on submit', () => {
        const properties = createProperties('String', {
            label: 'Label',
            maxLength: 10,
        });

        const form = createEditForm(properties);
        form.load(properties);

        const value = form.getValue()!;
        expect(value['label']).toEqual('Label');
        expect(value['maxLength']).toEqual(10);
        expect(value['minLength']).toBeUndefined();
        expect(value['pattern']).toBeUndefined();
        expect(value['isUnique']).toBe(false);
    });

    it('should only contain properties of the field type', () => {
        const form = createEditForm(createProperties('Boolean'));

        expect(Object.keys(form.model())).not.toContain('maxLength');
        expect(Object.keys(form.model())).toContain('inlineEditable');
    });

    it('should validate max length of label', () => {
        const form = createEditForm(createProperties('String'));

        form.form.label().value.set('a'.repeat(101));

        expect(form.form().valid()).toBeFalsy();
        expect(
            form.form
                .label()
                .errors()
                .map((x) => x.kind),
        ).toEqual(['maxLength']);
    });

    it('should hide string properties that do not apply to the editor', () => {
        const form = createEditForm(createProperties('String'));

        const stringForm = form.form as FieldTree<StringFieldFormModel>;
        stringForm.editor().value.set('Input');

        expect(stringForm.allowedValues().hidden()).toBeTruthy();
        expect(stringForm.classNames().hidden()).toBeTruthy();
        expect(stringForm.createEnum().hidden()).toBeTruthy();
        expect(stringForm.inlineEditable().hidden()).toBeFalsy();

        stringForm.editor().value.set('Dropdown');

        expect(stringForm.allowedValues().hidden()).toBeFalsy();
        expect(stringForm.createEnum().hidden()).toBeFalsy();

        stringForm.editor().value.set('RichText');

        expect(stringForm.classNames().hidden()).toBeFalsy();
        expect(stringForm.inlineEditable().hidden()).toBeTruthy();
    });

    it('should hide number properties that do not apply to the editor', () => {
        const form = createEditForm(createProperties('Number'));

        const numberForm = form.form as FieldTree<NumberFieldFormModel>;
        numberForm.editor().value.set('Radio');

        expect(numberForm.allowedValues().hidden()).toBeFalsy();
        expect(numberForm.inlineEditable().hidden()).toBeTruthy();

        numberForm.editor().value.set('Input');

        expect(numberForm.allowedValues().hidden()).toBeTruthy();
        expect(numberForm.inlineEditable().hidden()).toBeFalsy();
    });

    it('should not submit hidden properties', () => {
        const form = createEditForm(createProperties('String'));

        const stringForm = form.form as FieldTree<StringFieldFormModel>;
        stringForm.editor().value.set('Dropdown');
        stringForm.allowedValues().value.set(['a', 'b']);

        expect(form.getValue()!['allowedValues']).toEqual(['a', 'b']);

        stringForm.editor().value.set('Input');

        expect(form.getValue()!['allowedValues']).toBeUndefined();
    });

    it('should validate date time values', () => {
        const form = createEditForm(createProperties('DateTime'));

        const dateTimeForm = form.form as FieldTree<DateTimeFieldFormModel>;
        dateTimeForm.maxValue().value.set('invalid');

        expect(form.form().valid()).toBeFalsy();
        expect(
            dateTimeForm
                .maxValue()
                .errors()
                .map((x) => x.kind),
        ).toEqual(['validdatetime']);
    });

    it('should bind number input to number values', () => {
        const fixture = TestBed.createComponent(NumberHostComponent);
        fixture.detectChanges();

        const input: HTMLInputElement =
            fixture.nativeElement.querySelector('input');
        expect(input.value).toEqual('');

        input.value = '42';
        input.dispatchEvent(new Event('input'));
        fixture.detectChanges();

        expect(
            (fixture.componentInstance.editForm.model() as NumberFieldFormModel)
                .maxValue,
        ).toEqual(42);

        input.value = '';
        input.dispatchEvent(new Event('input'));
        fixture.detectChanges();

        expect(
            fixture.componentInstance.editForm.getValue()!['maxValue'],
        ).toBeUndefined();
    });
});

describe('AddFieldForm', () => {
    it('should detect content fields', () => {
        const form = TestBed.runInInjectionContext(() => new AddFieldForm());

        form.form.type().value.set('UI');
        expect(form.isContentField()).toBe(false);

        form.form.type().value.set('String');
        expect(form.isContentField()).toBe(true);
    });

    it('should create localizable field', () => {
        const form = TestBed.runInInjectionContext(() => new AddFieldForm());

        form.load({
            name: 'my-field',
            partitioning: 'language',
            properties: createProperties('Number'),
        });

        const value = form.getValue()!;
        expect(value.name).toEqual('my-field');
        expect(value.partitioning).toEqual('language');
        expect(value.properties.fieldType).toEqual('Number');
    });
});

describe('CreateSchemaForm', () => {
    it('should load and submit imported schema', () => {
        const form = TestBed.runInInjectionContext(
            () => new CreateSchemaForm(),
        );

        form.load({
            name: 'my-schema',
            type: 'Singleton',
            category: 'my-category',
            fields: [],
        } as any);

        expect(form.model().initialCategory).toEqual('my-category');
        expect(form.model().importing).toEqual({ fields: [] });

        const value = form.getValue()!;
        expect(value.name).toEqual('my-schema');
        expect(value.type).toEqual('Singleton');
        expect(value.category).toEqual('my-category');
    });

    it('should validate name', () => {
        const form = TestBed.runInInjectionContext(
            () => new CreateSchemaForm(),
        );

        form.form.name().value.set('Invalid Name');

        expect(form.form().valid()).toBeFalsy();
    });
});

describe('EditSchemaForm', () => {
    it('should limit search fields', () => {
        const form = TestBed.runInInjectionContext(() => new EditSchemaForm());

        form.form.searchFields().value.set(['a', 'b', 'c', 'd', 'e']);

        expect(form.form().valid()).toBeFalsy();
    });

    it('should convert empty urls to undefined', () => {
        const form = TestBed.runInInjectionContext(() => new EditSchemaForm());

        form.load({ label: 'Label', contentsSidebarUrl: null as any });

        const value = form.getValue()!;
        expect(value.label).toEqual('Label');
        expect(value.contentsSidebarUrl).toBeUndefined();
    });
});

describe('EditSchemaScriptsForm', () => {
    it('should load scripts with defaults', () => {
        const form = TestBed.runInInjectionContext(
            () => new EditSchemaScriptsForm(),
        );

        form.load({ query: 'query-script' });

        expect(form.model().query).toEqual('query-script');
        expect(form.model().create).toEqual('');
        expect(form.scripts).toContain('queryPre');
    });
});

describe('ConfigureFieldRulesForm', () => {
    it('should load, add and remove rules', () => {
        const form = TestBed.runInInjectionContext(
            () => new ConfigureFieldRulesForm(),
        );

        form.load({
            fieldRules: [new FieldRuleDto({ action: 'Hide', field: 'field1' })],
        } as Partial<SchemaDto>);
        form.add('field2');
        form.add('field3');
        form.remove(1);

        expect(form.model()).toMatchObject([
            { action: 'Hide', field: 'field1', condition: '' },
            { action: 'Disable', field: 'field3', condition: '' },
        ]);
    });

    it('should require field', () => {
        const form = TestBed.runInInjectionContext(
            () => new ConfigureFieldRulesForm(),
        );

        form.add();

        expect(form.form().valid()).toBeFalsy();
        expect(
            form.form[0]
                .field()
                .errors()
                .map((x) => x.kind),
        ).toEqual(['required']);
    });

    it('should submit rules', () => {
        const form = TestBed.runInInjectionContext(
            () => new ConfigureFieldRulesForm(),
        );

        form.add('field1');

        const value = form.getValue()!;
        expect(value.fieldRules?.map((x) => x.field)).toEqual(['field1']);
    });
});

describe('ConfigurePreviewUrlsForm', () => {
    it('should load and submit preview urls', () => {
        const form = TestBed.runInInjectionContext(
            () => new ConfigurePreviewUrlsForm(),
        );

        form.load({
            previewUrls: { web: 'https://web' },
        } as Partial<SchemaDto>);
        form.add();
        form.form[1].name().value.set('app');
        form.form[1].url().value.set('https://app');

        expect(form.getValue()).toEqual({
            web: 'https://web',
            app: 'https://app',
        });
    });

    it('should remove preview url', () => {
        const form = TestBed.runInInjectionContext(
            () => new ConfigurePreviewUrlsForm(),
        );

        form.load({
            previewUrls: { web: 'https://web', app: 'https://app' },
        } as Partial<SchemaDto>);
        form.remove(0);

        expect(form.model()).toMatchObject([
            { name: 'app', url: 'https://app' },
        ]);
    });
});

describe('CreateIndexForm', () => {
    it('should add and remove fields', () => {
        const form = TestBed.runInInjectionContext(() => new CreateIndexForm());

        form.add();
        form.add();
        form.form[0].name().value.set('data.field1.iv');
        form.form[0].order().value.set('Ascending');
        form.remove(1);

        const value = form.getValue()!;
        expect(value.fields.map((x) => [x.name, x.order])).toEqual([
            ['data.field1.iv', 'Ascending'],
        ]);
    });

    it('should require name and order', () => {
        const form = TestBed.runInInjectionContext(() => new CreateIndexForm());

        form.add();

        expect(form.form().valid()).toBeFalsy();
    });
});

@Component({
    imports: [FormField],
    template: ` <input [formField]="numberForm.maxValue" type="number" /> `,
})
class NumberHostComponent {
    public readonly editForm = new EditFieldForm(createProperties('Number'));

    public readonly numberForm = this.editForm
        .form as FieldTree<NumberFieldFormModel>;
}

function createEditForm(properties: ReturnType<typeof createProperties>) {
    return TestBed.runInInjectionContext(() => new EditFieldForm(properties));
}
