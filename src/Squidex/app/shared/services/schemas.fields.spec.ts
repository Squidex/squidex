/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from '@app/framework';

import {
    AssetsFieldPropertiesDto,
    BooleanFieldPropertiesDto,
    DateTimeFieldPropertiesDto,
    FieldDto,
    FieldPropertiesDto,
    GeolocationFieldPropertiesDto,
    JsonFieldPropertiesDto,
    NumberFieldPropertiesDto,
    ReferencesFieldPropertiesDto,
    SchemaDetailsDto,
    SchemaPropertiesDto,
    StringFieldPropertiesDto,
    TagsFieldPropertiesDto
} from './../';

describe('SchemaDetailsDto', () => {
    it('should return label as display name', () => {
        const schema = createSchema(new SchemaPropertiesDto('Label'), 1, []);

        expect(schema.displayName).toBe('Label');
    });

    it('should return name as display name if label is undefined', () => {
        const schema = createSchema(new SchemaPropertiesDto(undefined), 1, []);

        expect(schema.displayName).toBe('schema1');
    });

    it('should return name as display name label is empty', () => {
        const schema = createSchema(new SchemaPropertiesDto(''), 1, []);

        expect(schema.displayName).toBe('schema1');
    });

    it('should return configured fields as list fields if no schema field are declared', () => {
        const field1 = createField(new AssetsFieldPropertiesDto(null, null, null, false, true), 1);
        const field2 = createField(new AssetsFieldPropertiesDto(null, null, null, false, false), 2);
        const field3 = createField(new AssetsFieldPropertiesDto(null, null, null, false, true), 3);

        const schema = createSchema(new SchemaPropertiesDto(''), 1, [field1, field2, field3]);

        expect(schema.listFields).toEqual([field1, field3]);
    });

    it('should return first fields as list fields if no schema field is declared', () => {
        const field1 = createField(new AssetsFieldPropertiesDto(null, null, null, false, false), 1);
        const field2 = createField(new AssetsFieldPropertiesDto(null, null, null, false, false), 2);
        const field3 = createField(new AssetsFieldPropertiesDto(null, null, null, false, false), 3);

        const schema = createSchema(new SchemaPropertiesDto(''), 1, [field1, field2, field3]);

        expect(schema.listFields).toEqual([field1]);
    });

    it('should return empty list fields if fields is empty', () => {
        const schema = createSchema(new SchemaPropertiesDto(''), 1, []);

        expect(schema.listFields).toEqual([{ properties: {} }]);
    });
});

describe('FieldDto', () => {
    it('should return label as display name', () => {
        const field = createField(new AssetsFieldPropertiesDto('Label', null, null, true, false), 1);

        expect(field.displayName).toBe('Label');
    });

    it('should return name as display name if label is null', () => {
        const field = createField(new AssetsFieldPropertiesDto(null, null, null, true, false), 1);

        expect(field.displayName).toBe('field1');
    });

    it('should return name as display name label is empty', () => {
        const field = createField(new AssetsFieldPropertiesDto('', null, null, true, false), 1);

        expect(field.displayName).toBe('field1');
    });

    it('should return placeholder as display placeholder', () => {
        const field = createField(new AssetsFieldPropertiesDto(null, null, 'Placeholder', true, false), 1);

        expect(field.displayPlaceholder).toBe('Placeholder');
    });

    it('should return empty as display placeholder if placeholder is null', () => {
        const field = createField(new AssetsFieldPropertiesDto(null, null, null, true, false));

        expect(field.displayPlaceholder).toBe('');
    });

    it('should return localizable if partitioning is language', () => {
        const field = createField(new AssetsFieldPropertiesDto(null, null, null, true, false), 1, 'language');

        expect(field.isLocalizable).toBeTruthy();
    });

    it('should not return localizable if partitioning is invarient', () => {
        const field = createField(new AssetsFieldPropertiesDto(null, null, null, true, false), 1, 'invariant');

        expect(field.isLocalizable).toBeFalsy();
    });
});

describe('AssetsField', () => {
    const field = createField(new AssetsFieldPropertiesDto(null, null, null, true, false, 1, 1));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to asset count', () => {
        expect(field.formatValue([1, 2, 3])).toBe('3 Asset(s)');
    });

    it('should return zero formatting if other type', () => {
        expect(field.formatValue(1)).toBe('0 Assets');
    });

    it('should return null for default properties', () => {
        expect(field.defaultValue()).toBeNull();
    });
});

describe('TagsField', () => {
    const field = createField(new TagsFieldPropertiesDto(null, null, null, true, false, 1, 1));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to asset count', () => {
        expect(field.formatValue(['hello', 'squidex', 'cms'])).toBe('hello, squidex, cms');
    });

    it('should return zero formatting if other type', () => {
        expect(field.formatValue(1)).toBe('');
    });

    it('should return null for default properties', () => {
        expect(field.defaultValue()).toBeNull();
    });
});

describe('BooleanField', () => {
    const field = createField(new BooleanFieldPropertiesDto(null, null, null, true, false, false, 'Checkbox'));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to Yes if true', () => {
        expect(field.formatValue(true)).toBe('Yes');
    });

    it('should format to No if false', () => {
        expect(field.formatValue(false)).toBe('No');
    });

    it('should return default value for default properties', () => {
        Object.assign(field.properties, { defaultValue: true });

        expect(field.defaultValue()).toBeTruthy();
    });
});

describe('DateTimeField', () => {
    const now = DateTime.parseISO_UTC('2017-10-12T16:30:10Z');
    const field = createField(new DateTimeFieldPropertiesDto(null, null, null, true, false, 'Date'));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to input if parsing failed', () => {
        expect(field.formatValue(true)).toBe(true);
    });

    it('should format to date', () => {
        const dateField = createField(new DateTimeFieldPropertiesDto(null, null, null, true, false, 'Date'));

        expect(dateField.formatValue('2017-12-12T16:00:00Z')).toBe('2017-12-12');
    });

    it('should format to date time', () => {
        const dateTimeField = createField(new DateTimeFieldPropertiesDto(null, null, null, true, false, 'DateTime'));

        expect(dateTimeField.formatValue('2017-12-12T16:00:00Z')).toBe('2017-12-12 16:00:00');
    });

    it('should return default for DateFieldProperties', () => {
        Object.assign(field.properties, { defaultValue: '2017-10-12T16:00:00Z' });

        expect(field.defaultValue()).toEqual('2017-10-12T16:00:00Z');
    });

    it('should return calculated date when Today for DateFieldProperties', () => {
        Object.assign(field.properties, { calculatedDefaultValue: 'Today' });

        expect((<any>field).properties.getDefaultValue(now)).toEqual('2017-10-12');
    });

    it('should return calculated date when Now for DateFieldProperties', () => {
        Object.assign(field.properties, { calculatedDefaultValue: 'Now' });

        expect((<any>field).properties.getDefaultValue(now)).toEqual('2017-10-12T16:30:10Z');
    });
});

describe('GeolocationField', () => {
    const field = createField(new GeolocationFieldPropertiesDto(null, null, null, true, false, 'Default'));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to latitude and longitude', () => {
        expect(field.formatValue({ latitude: 42, longitude: 3.14 })).toBe('3.14, 42');
    });

    it('should return null for default properties', () => {
        expect(field.defaultValue()).toBeNull();
    });
});

describe('JsonField', () => {
    const field = createField(new JsonFieldPropertiesDto(null, null, null, true, false));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to constant', () => {
        expect(field.formatValue({})).toBe('<Json />');
    });

    it('should return null for default properties', () => {
        expect(field.defaultValue()).toBeNull();
    });
});

describe('NumberField', () => {
    const field = createField(new NumberFieldPropertiesDto(null, null, null, true, false, false, 'Input', undefined, 3, 1, [1, 2, 3]));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(4);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to number', () => {
        expect(field.formatValue(42)).toBe(42);
    });

    it('should return default value for default properties', () => {
        Object.assign(field.properties, { defaultValue: 13 });

        expect(field.defaultValue()).toEqual(13);
    });
});

describe('ReferencesField', () => {
    const field = createField(new ReferencesFieldPropertiesDto(null, null, null, true, false, 1, 1));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to asset count', () => {
        expect(field.formatValue([1, 2, 3])).toBe('3 Reference(s)');
    });

    it('should return zero formatting if other type', () => {
        expect(field.formatValue(1)).toBe('0 References');
    });

    it('should return null for default properties', () => {
        expect(field.defaultValue()).toBeNull();
    });
});

describe('StringField', () => {
    const field = createField(new StringFieldPropertiesDto(null, null, null, true, false, false, 'Input', undefined, 'pattern', undefined, 3, 1, ['1', '2']));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(5);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to string', () => {
        expect(field.formatValue('hello')).toBe('hello');
    });

    it('should return default value for default properties', () => {
        Object.assign(field.properties, { defaultValue: 'MyDefault' });

        expect(field.defaultValue()).toEqual('MyDefault');
    });
});

function createSchema(properties: SchemaPropertiesDto, index = 1, fields: FieldDto[]) {
    return new SchemaDetailsDto('id' + index, 'schema' + index, properties, true, null!, null!, null!, null!, null!, fields);
}

function createField(properties: FieldPropertiesDto, index = 1, partitioning = 'languages') {
    return new FieldDto(index, 'field' + index, false, false, false, partitioning, properties);
}