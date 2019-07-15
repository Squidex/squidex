/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import {
    ArrayFieldPropertiesDto,
    AssetsFieldPropertiesDto,
    BooleanFieldPropertiesDto,
    DateTime,
    DateTimeFieldPropertiesDto,
    FieldDefaultValue,
    FieldFormatter,
    FieldPropertiesDto,
    FieldValidatorsFactory,
    GeolocationFieldPropertiesDto,
    JsonFieldPropertiesDto,
    NumberFieldPropertiesDto,
    ReferencesFieldPropertiesDto,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaPropertiesDto,
    StringFieldPropertiesDto,
    TagsFieldPropertiesDto
} from '@app/shared/internal';
import { HtmlValue } from './contents.forms';

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
        const field1 = createField(new ArrayFieldPropertiesDto({ isListField: true }), 1);
        const field2 = createField(new ArrayFieldPropertiesDto({ isListField: false }), 2);
        const field3 = createField(new ArrayFieldPropertiesDto({ isListField: true }), 3);

        const schema = createSchema(new SchemaPropertiesDto(''), 1, [field1, field2, field3]);

        expect(schema.listFields).toEqual([field1, field3]);
    });

    it('should return first fields as list fields if no schema field is declared', () => {
        const field1 = createField(new ArrayFieldPropertiesDto(), 1);
        const field2 = createField(new ArrayFieldPropertiesDto(), 2);
        const field3 = createField(new ArrayFieldPropertiesDto(), 3);

        const schema = createSchema(new SchemaPropertiesDto(''), 1, [field1, field2, field3]);

        expect(schema.listFields).toEqual([field1]);
    });

    it('should return empty list fields if fields is empty', () => {
        const schema = createSchema(new SchemaPropertiesDto(), 1, []);

        expect(schema.listFields[0].fieldId).toEqual(-1);
    });
});

describe('FieldDto', () => {
    it('should return label as display name', () => {
        const field = createField(new AssetsFieldPropertiesDto({ label: 'Label' }), 1);

        expect(field.displayName).toBe('Label');
    });

    it('should return name as display name if label is null', () => {
        const field = createField(new AssetsFieldPropertiesDto(), 1);

        expect(field.displayName).toBe('field1');
    });

    it('should return name as display name label is empty', () => {
        const field = createField(new AssetsFieldPropertiesDto({ label: '' }), 1);

        expect(field.displayName).toBe('field1');
    });

    it('should return placeholder as display placeholder', () => {
        const field = createField(new AssetsFieldPropertiesDto({ placeholder: 'Placeholder' }), 1);

        expect(field.displayPlaceholder).toBe('Placeholder');
    });

    it('should return empty as display placeholder if placeholder is null', () => {
        const field = createField(new AssetsFieldPropertiesDto());

        expect(field.displayPlaceholder).toBe('');
    });

    it('should return localizable if partitioning is language', () => {
        const field = createField(new AssetsFieldPropertiesDto(), 1, 'language');

        expect(field.isLocalizable).toBeTruthy();
    });

    it('should not return localizable if partitioning is invarient', () => {
        const field = createField(new AssetsFieldPropertiesDto(), 1, 'invariant');

        expect(field.isLocalizable).toBeFalsy();
    });
});

describe('ArrayField', () => {
    const field = createField(new ArrayFieldPropertiesDto({ isRequired: true, minItems: 1, maxItems: 5 }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(2);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to asset count', () => {
        expect(FieldFormatter.format(field, [1, 2, 3])).toBe('3 Item(s)');
    });

    it('should return zero formatting if other type', () => {
        expect(FieldFormatter.format(field, 1)).toBe('0 Items');
    });

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
    });
});

describe('AssetsField', () => {
    const field = createField(new AssetsFieldPropertiesDto({ isRequired: true, minItems: 1, maxItems: 5 }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to asset count', () => {
        expect(FieldFormatter.format(field, [1, 2, 3])).toBe('3 Asset(s)');
    });

    it('should return zero formatting if other type', () => {
        expect(FieldFormatter.format(field, 1)).toBe('0 Assets');
    });

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
    });
});

describe('TagsField', () => {
    const field = createField(new TagsFieldPropertiesDto('Tags', { isRequired: true, minItems: 1, maxItems: 5 }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(2);
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

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
    });
});

describe('BooleanField', () => {
    const field = createField(new BooleanFieldPropertiesDto('Checkbox', { isRequired: true }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(1);
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

    it('should return default value for default properties', () => {
        const field2 = createField(new BooleanFieldPropertiesDto('Checkbox', { defaultValue: true }));

        expect(FieldDefaultValue.get(field2)).toBeTruthy();
    });
});

describe('DateTimeField', () => {
    const now = DateTime.parseISO_UTC('2017-10-12T16:30:10Z');
    const field = createField(new DateTimeFieldPropertiesDto('DateTime', { isRequired: true }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to input if parsing failed', () => {
        expect(FieldFormatter.format(field, true)).toBe(true);
    });

    it('should format to date', () => {
        const dateField = createField(new DateTimeFieldPropertiesDto('Date'));

        expect(FieldFormatter.format(dateField, '2017-12-12T16:00:00Z')).toBe('2017-12-12');
    });

    it('should format to date time', () => {
        const field2 = createField(new DateTimeFieldPropertiesDto('DateTime'));

        expect(FieldFormatter.format(field2, '2017-12-12T16:00:00Z')).toBe('2017-12-12 16:00:00');
    });

    it('should return default for DateFieldProperties', () => {
        const field2 = createField(new DateTimeFieldPropertiesDto('DateTime', { defaultValue: '2017-10-12T16:00:00Z' }));

        expect(FieldDefaultValue.get(field2)).toEqual('2017-10-12T16:00:00Z');
    });

    it('should return calculated date when Today for DateFieldProperties', () => {
        const field2 = createField(new DateTimeFieldPropertiesDto('DateTime', { calculatedDefaultValue: 'Today' }));

        expect(FieldDefaultValue.get(field2, now)).toEqual('2017-10-12');
    });

    it('should return calculated date when Now for DateFieldProperties', () => {
        const field2 = createField(new DateTimeFieldPropertiesDto('DateTime', { calculatedDefaultValue: 'Now' }));

        expect(FieldDefaultValue.get(field2, now)).toEqual('2017-10-12T16:30:10Z');
    });
});

describe('GeolocationField', () => {
    const field = createField(new GeolocationFieldPropertiesDto({ isRequired: true }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to latitude and longitude', () => {
        expect(FieldFormatter.format(field, { latitude: 42, longitude: 3.14 })).toBe('3.14, 42');
    });

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
    });
});

describe('JsonField', () => {
    const field = createField(new JsonFieldPropertiesDto({ isRequired: true }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to constant', () => {
        expect(FieldFormatter.format(field, {})).toBe('<Json />');
    });

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
    });
});

describe('NumberField', () => {
    const field = createField(new NumberFieldPropertiesDto('Input', { isRequired: true, minValue: 1, maxValue: 6, allowedValues: [1, 3] }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to number', () => {
        expect(FieldFormatter.format(field, 42)).toEqual(42);
    });

    it('should format to stars if html allowed', () => {
        const field2 = createField(new NumberFieldPropertiesDto('Stars'));

        expect(FieldFormatter.format(field2, 3)).toEqual(new HtmlValue('&#9733; &#9733; &#9733; '));
    });

    it('should format to short star view for many stars', () => {
        const field2 = createField(new NumberFieldPropertiesDto('Stars'));

        expect(FieldFormatter.format(field2, 42)).toEqual(new HtmlValue('&#9733; 42'));
    });

    it('should format to short star view for no stars', () => {
        const field2 = createField(new NumberFieldPropertiesDto('Stars'));

        expect(FieldFormatter.format(field2, 0)).toEqual(new HtmlValue('&#9733; 0'));
    });

    it('should format to short star view for negative stars', () => {
        const field2 = createField(new NumberFieldPropertiesDto('Stars'));

        expect(FieldFormatter.format(field2, -13)).toEqual(new HtmlValue('&#9733; -13'));
    });

    it('should not format to stars if html not allowed', () => {
        const field2 = createField(new NumberFieldPropertiesDto('Stars'));

        expect(FieldFormatter.format(field2, 3, false)).toEqual(3);
    });

    it('should return default value for default properties', () => {
        const field2 = createField(new NumberFieldPropertiesDto('Input', { defaultValue: 13 }));

        expect(FieldDefaultValue.get(field2)).toEqual(13);
    });
});

describe('ReferencesField', () => {
    const field = createField(new ReferencesFieldPropertiesDto('List', { isRequired: true, minItems: 1, maxItems: 5 }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(3);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to asset count', () => {
        expect(FieldFormatter.format(field, [1, 2, 3])).toBe('3 Reference(s)');
    });

    it('should return zero formatting if other type', () => {
        expect(FieldFormatter.format(field, 1)).toBe('0 References');
    });

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
    });
});

describe('StringField', () => {
    const field = createField(new StringFieldPropertiesDto('Input', { isRequired: true, pattern: 'pattern', minLength: 1, maxLength: 5, allowedValues: ['a', 'b'] }));

    it('should create validators', () => {
        expect(FieldValidatorsFactory.createValidators(field, false).length).toBe(4);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to string', () => {
        expect(FieldFormatter.format(field, 'hello')).toBe('hello');
    });

    it('should return default value for default properties', () => {
        const field2 = createField(new StringFieldPropertiesDto('Input', { defaultValue: 'MyDefault' }));

        expect(FieldDefaultValue.get(field2)).toEqual('MyDefault');
    });
});

function createSchema(properties: SchemaPropertiesDto, index = 1, fields: RootFieldDto[]) {
    return new SchemaDetailsDto({}, 'id' + index, 'schema' + index, '', properties, false, true, null!, null!, null!, null!, null!, fields);
}

function createField(properties: FieldPropertiesDto, index = 1, partitioning = 'languages') {
    return new RootFieldDto({}, index, 'field' + index, properties, partitioning);
}