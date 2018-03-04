/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DateTime } from 'framework';

import {
    AssetsFieldPropertiesDto,
    BooleanFieldPropertiesDto,
    createProperties,
    DateTimeFieldPropertiesDto,
    FieldDto,
    FieldPropertiesDto,
    GeolocationFieldPropertiesDto,
    JsonFieldPropertiesDto,
    NumberFieldPropertiesDto,
    ReferencesFieldPropertiesDto,
    StringFieldPropertiesDto,
    TagsFieldPropertiesDto
} from './../';

describe('FieldDto', () => {
    it('should update isLocked property when locking', () => {
        const field_1 = createField(createProperties('String'));
        const field_2 = field_1.lock();

        expect(field_2.isLocked).toBeTruthy();
    });

    it('should update isHidden property when hiding', () => {
        const field_1 = createField(createProperties('String'));
        const field_2 = field_1.hide();

        expect(field_2.isHidden).toBeTruthy();
    });

    it('should update isHidden property when showing', () => {
        const field_1 = createField(createProperties('String')).hide();
        const field_2 = field_1.show();

        expect(field_2.isHidden).toBeFalsy();
    });

    it('should update isDisabled property when disabling', () => {
        const field_1 = createField(createProperties('String'));
        const field_2 = field_1.disable();

        expect(field_2.isDisabled).toBeTruthy();
    });

    it('should update isDisabled property when enabling', () => {
        const field_1 = createField(createProperties('String')).disable();
        const field_2 = field_1.enable();

        expect(field_2.isDisabled).toBeFalsy();
    });

    it('should update properties property when updating', () => {
        const newProperty = createProperties('Number');

        const field_1 = createField(createProperties('String'));
        const field_2 = field_1.update(newProperty);

        expect(field_2.properties).toEqual(newProperty);
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
        Object.assign(field.properties, { defaultValue : true });

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

function createField(properties: FieldPropertiesDto) {
    return new FieldDto(1, 'field1', false, false, false, 'languages', properties);
}