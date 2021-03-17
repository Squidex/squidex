/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { DateHelper } from '@app/framework';
import { createProperties, DateTime, FieldDefaultValue, FieldFormatter, FieldsValidators, HtmlValue, MetaFields, SchemaPropertiesDto } from '@app/shared/internal';
import { TestValues } from './_test-helpers';

const {
    createField,
    createSchema
} = TestValues;

const now = DateTime.parseISO('2017-10-12T16:30:10Z');

describe('SchemaDetailsDto', () => {
    const field1 = createField({ properties: createProperties('Array'), id: 1 });
    const field2 = createField({ properties: createProperties('Array'), id: 2 });
    const field3 = createField({ properties: createProperties('Array'), id: 3 });

    it('should return label as display name', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto('Label') });

        expect(schema.displayName).toBe('Label');
    });

    it('should return name as display name if label is undefined', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(undefined) });

        expect(schema.displayName).toBe('schema1');
    });

    it('should return name as display name label is empty', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto('') });

        expect(schema.displayName).toBe('schema1');
    });

    it('should return configured fields as list fields if fields are declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3], fieldsInLists: ['field1', 'field3'] });

        expect(schema.defaultListFields).toEqual([field1, field3]);
    });

    it('should return first fields as list fields if no field is declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3] });

        expect(schema.defaultListFields).toEqual([MetaFields.lastModifiedByAvatar, field1, MetaFields.statusColor, MetaFields.lastModified]);
    });

    it('should return preset with empty content field as list fields if fields is empty', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto() });

        expect(schema.defaultListFields).toEqual([MetaFields.lastModifiedByAvatar, '', MetaFields.statusColor, MetaFields.lastModified]);
    });

    it('should return configured fields as references fields if fields are declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3], fieldsInReferences: ['field1', 'field3'] });

        expect(schema.defaultReferenceFields).toEqual([field1, field3]);
    });

    it('should return first field as reference fields if no field is declared', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto(''), fields: [field1, field2, field3] });

        expect(schema.defaultReferenceFields).toEqual([field1]);
    });

    it('should return noop field as reference field if list is empty', () => {
        const schema = createSchema({ properties: new SchemaPropertiesDto() });

        expect(schema.defaultReferenceFields).toEqual(['']);
    });
});

describe('FieldDto', () => {
    it('should return label as display name', () => {
        const field = createField({ properties: createProperties('Array', { label: 'Label' }) });

        expect(field.displayName).toBe('Label');
    });

    it('should return name as display name if label is null', () => {
        const field = createField({ properties: createProperties('Assets') });

        expect(field.displayName).toBe('field1');
    });

    it('should return name as display name label is empty', () => {
        const field = createField({ properties: createProperties('Assets', { label: '' }) });

        expect(field.displayName).toBe('field1');
    });

    it('should return placeholder as display placeholder', () => {
        const field = createField({ properties: createProperties('Assets', { placeholder: 'Placeholder' }) });

        expect(field.displayPlaceholder).toBe('Placeholder');
    });

    it('should return empty as display placeholder if placeholder is null', () => {
        const field = createField({ properties: createProperties('Assets') });

        expect(field.displayPlaceholder).toBe('');
    });

    it('should return localizable if partitioning is language', () => {
        const field = createField({ properties: createProperties('Assets'), partitioning: 'language' });

        expect(field.isLocalizable).toBeTruthy();
    });

    it('should not return localizable if partitioning is invariant', () => {
        const field = createField({ properties: createProperties('Assets'), partitioning: 'invariant' });

        expect(field.isLocalizable).toBeFalsy();
    });
});

describe('ArrayField', () => {
    const field = createField({ properties: createProperties('Array', { isRequired: true, minItems: 1, maxItems: 5 }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(2);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to plural count for many items', () => {
        expect(FieldFormatter.format(field, [1, 2, 3])).toBe('3 Items');
    });

    it('should format to plural count for single item', () => {
        expect(FieldFormatter.format(field, [1])).toBe('1 Item');
    });

    it('should return zero formatting if other type', () => {
        expect(FieldFormatter.format(field, 1)).toBe('0 Items');
    });

    it('should return default value as null', () => {
        expect(FieldDefaultValue.get(field, 'iv')).toBeNull();
    });
});

describe('AssetsField', () => {
    const field = createField({ properties: createProperties('Assets', { isRequired: true, minItems: 1, maxItems: 5 }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to plural count for many items', () => {
        expect(FieldFormatter.format(field, [1, 2, 3])).toBe('3 Assets');
    });

    it('should format to plural count for single item', () => {
        expect(FieldFormatter.format(field, [1])).toBe('1 Asset');
    });

    it('should return zero formatting if other type', () => {
        expect(FieldFormatter.format(field, 1)).toBe('0 Assets');
    });

    it('should return default value from properties', () => {
        const field2 = createField({ properties: createProperties('Assets', { defaultValue: ['1', '2'] }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toEqual(['1', '2']);
    });

    it('should override default value from localizable properties', () => {
        const field2 = createField({ properties: createProperties('Assets', { defaultValue: ['1', '2'], defaultValues: { 'iv': null } }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeNull();
    });
});

describe('TagsField', () => {
    const field = createField({ properties: createProperties('Tags', { isRequired: true, minItems: 1, maxItems: 5 }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(2);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to asset count', () => {
        expect(FieldFormatter.format(field, ['hello', 'squidex', 'cms'])).toBe('hello, squidex, cms');
    });

    it('should return zero formatting if other type', () => {
        expect(FieldFormatter.format(field, 1)).toBe('');
    });

    it('should return default value from properties', () => {
        const field2 = createField({ properties: createProperties('Tags', { defaultValue: ['1', '2'] }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toEqual(['1', '2']);
    });

    it('should override default value from localizable properties', () => {
        const field2 = createField({ properties: createProperties('Tags', { defaultValue: ['1', '2'], defaultValues: { 'iv': null } }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeNull();
    });
});

describe('BooleanField', () => {
    const field = createField({ properties: createProperties('Boolean', { editor: 'Checkbox', isRequired: true }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to Yes if true', () => {
        expect(FieldFormatter.format(field, true)).toBe('Yes');
    });

    it('should format to No if false', () => {
        expect(FieldFormatter.format(field, false)).toBe('No');
    });

    it('should return default value from properties', () => {
        const field2 = createField({ properties: createProperties('Boolean', { editor: 'Checkbox', defaultValue: true }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeTruthy();
    });

    it('should override default value from localizable properties', () => {
        const field2 = createField({ properties: createProperties('Boolean', { defaultValue: true, defaultValues: { 'iv': null } }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeNull();
    });
});

describe('DateTimeField', () => {
    const field = createField({ properties: createProperties('DateTime', { editor: 'DateTime', isRequired: true }) });

    beforeEach(() => {
        DateHelper.setlocale(null);
    });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to input if parsing failed', () => {
        expect(FieldFormatter.format(field, true)).toBe(true);
    });

    it('should format old format to date', () => {
        const dateField = createField({ properties: createProperties('DateTime', { editor: 'Date' }) });

        expect(FieldFormatter.format(dateField, '2017-12-12')).toBe('12/12/2017');
    });

    it('should format datetime to date', () => {
        const dateField = createField({ properties: createProperties('DateTime', { editor: 'Date' }) });

        expect(FieldFormatter.format(dateField, '2017-12-12T16:00:00Z')).toBe('12/12/2017');
    });

    it('should format date to date', () => {
        const dateField = createField({ properties: createProperties('DateTime', { editor: 'Date' }) });

        expect(FieldFormatter.format(dateField, '2017-12-12T00:00:00Z')).toBe('12/12/2017');
    });

    it('should format to date time', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime' }) });

        expect(FieldFormatter.format(field2, '2017-12-12T16:00:00Z')).toBe('12/12/2017, 4:00:00 PM');
    });

    it('should return default from properties value', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime', defaultValue: '2017-10-12T16:00:00Z' }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toEqual('2017-10-12T16:00:00Z');
    });

    it('should override default value from localizable properties', () => {
        const field2 = createField({ properties: createProperties('DateTime', { defaultValue: '2017-10-12T16:00:00Z', defaultValues: { 'iv': null } }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeNull();
    });

    it('should return default from Today', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime', calculatedDefaultValue: 'Today' }) });

        expect(FieldDefaultValue.get(field2, 'iv', now)).toEqual('2017-10-12T00:00:00Z');
    });

    it('should return default value from Today', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime', calculatedDefaultValue: 'Now' }) });

        expect(FieldDefaultValue.get(field2, 'iv', now)).toEqual('2017-10-12T16:30:10Z');
    });
});

describe('GeolocationField', () => {
    const field = createField({ properties: createProperties('Geolocation', { isRequired: true }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to latitude and longitude', () => {
        expect(FieldFormatter.format(field, { latitude: 42, longitude: 3.14 })).toBe('3.14, 42');
    });

    it('should return default value as null', () => {
        expect(FieldDefaultValue.get(field, 'iv')).toBeNull();
    });
});

describe('JsonField', () => {
    const field = createField({ properties: createProperties('Json', { isRequired: true }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to constant', () => {
        expect(FieldFormatter.format(field, {})).toBe('<Json />');
    });

    it('should return default value as null', () => {
        expect(FieldDefaultValue.get(field, 'iv')).toBeNull();
    });
});

describe('NumberField', () => {
    const field = createField({ properties: createProperties('Number', { isRequired: true, minValue: 1, maxValue: 6, allowedValues: [1, 3] }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to number', () => {
        expect(FieldFormatter.format(field, 42)).toEqual('42');
    });

    it('should format to stars if html allowed', () => {
        const field2 = createField({ properties: createProperties('Number', { editor: 'Stars' }) });

        expect(FieldFormatter.format(field2, 3)).toEqual(new HtmlValue('&#9733; &#9733; &#9733; '));
    });

    it('should format to short star view for many stars', () => {
        const field2 = createField({ properties: createProperties('Number', { editor: 'Stars' }) });

        expect(FieldFormatter.format(field2, 42)).toEqual(new HtmlValue('&#9733; 42'));
    });

    it('should format to short star view for no stars', () => {
        const field2 = createField({ properties: createProperties('Number', { editor: 'Stars' }) });

        expect(FieldFormatter.format(field2, 0)).toEqual(new HtmlValue('&#9733; 0'));
    });

    it('should format to short star view for negative stars', () => {
        const field2 = createField({ properties: createProperties('Number', { editor: 'Stars' }) });

        expect(FieldFormatter.format(field2, -13)).toEqual(new HtmlValue('&#9733; -13'));
    });

    it('should not format to stars if html not allowed', () => {
        const field2 = createField({ properties: createProperties('Number', { editor: 'Stars' }) });

        expect(FieldFormatter.format(field2, 3, false)).toEqual('3');
    });

    it('should return default value from properties', () => {
        const field2 = createField({ properties: createProperties('Number', { defaultValue: 13 }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toEqual(13);
    });

    it('should override default value from localizable properties', () => {
        const field2 = createField({ properties: createProperties('Number', { defaultValue: 13, defaultValues: { 'iv': null } }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeNull();
    });
});

describe('ReferencesField', () => {
    const field = createField({ properties: createProperties('References', { editor: 'List', isRequired: true, minItems: 1, maxItems: 5 }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to plural count for many items', () => {
        expect(FieldFormatter.format(field, [1, 2, 3])).toBe('3 References');
    });

    it('should format to plural count for single item', () => {
        expect(FieldFormatter.format(field, [1])).toBe('1 Reference');
    });

    it('should return zero formatting if other type', () => {
        expect(FieldFormatter.format(field, 1)).toBe('0 References');
    });

    it('should return default value from properties', () => {
        const field2 = createField({ properties: createProperties('References', { defaultValue: ['1', '2'] }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toEqual(['1', '2']);
    });

    it('should override default value from localizable properties', () => {
        const field2 = createField({ properties: createProperties('References', { defaultValue: ['1', '2'], defaultValues: { 'iv': null } }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeNull();
    });
});

describe('StringField', () => {
    const field = createField({ properties: createProperties('String', { isRequired: true, pattern: 'pattern', minLength: 1, maxLength: 5, allowedValues: ['a', 'b'] }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(4);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to string', () => {
        expect(FieldFormatter.format(field, 'hello')).toBe('hello');
    });

    it('should format to preview image', () => {
        const field2 = createField({ properties: createProperties('String', { editor: 'StockPhoto' }) });

        expect(FieldFormatter.format(field2, 'https://images.unsplash.com/123?x', true)).toEqual(new HtmlValue('<img src="https://images.unsplash.com/123?x&q=80&fm=jpg&crop=entropy&cs=tinysrgb&h=50&fit=max" />'));
    });

    it('should not format to preview image when html not allowed', () => {
        const field2 = createField({ properties: createProperties('String', { editor: 'StockPhoto' }) });

        expect(FieldFormatter.format(field2, 'https://images.unsplash.com/123?x', false)).toBe('https://images.unsplash.com/123?x');
    });

    it('should not format to preview image when not unsplash image', () => {
        const field2 = createField({ properties: createProperties('String', { editor: 'StockPhoto' }) });

        expect(FieldFormatter.format(field2, 'https://images.com/123?x', true)).toBe('https://images.com/123?x');
    });

    it('should return default value from properties', () => {
        const field2 = createField({ properties: createProperties('String', { defaultValue: 'MyDefault' }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toEqual('MyDefault');
    });

    it('should override default value from localizable properties', () => {
        const field2 = createField({ properties: createProperties('String', { defaultValue: 'MyDefault', defaultValues: { 'iv': null } }) });

        expect(FieldDefaultValue.get(field2, 'iv')).toBeNull();
    });
});