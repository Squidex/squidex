/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

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
    StringFieldPropertiesDto
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
});

describe('BooleanField', () => {
    const field = createField(new BooleanFieldPropertiesDto(null, null, null, true, false, 'Checkbox'));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to checkmark if true', () => {
        expect(field.formatValue(true)).toBe('✔');
    });

    it('should format to minus if false', () => {
        expect(field.formatValue(false)).toBe('-');
    });
});

describe('DateTimeField', () => {
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

    it('should format to date', () => {
        const dateTimeField = createField(new DateTimeFieldPropertiesDto(null, null, null, true, false, 'DateTime'));

        expect(dateTimeField.formatValue('2017-12-12T16:00:00Z').substr(0, 10)).toBe('2017-12-12');
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
});

describe('NumberField', () => {
    const field = createField(new NumberFieldPropertiesDto(null, null, null, true, false, 'Input', undefined, 3, 1, [1, 2, 3]));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(4);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to number', () => {
        expect(field.formatValue(42)).toBe(42);
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
});

describe('NumberField', () => {
    const field = createField(new StringFieldPropertiesDto(null, null, null, true, false, 'Input', undefined, 'pattern', undefined, 3, 1, ['1', '2']));

    it('should create validators', () => {
        expect(field.createValidators(false).length).toBe(5);
    });

    it('should format to empty string if null', () => {
        expect(field.formatValue(null)).toBe('');
    });

    it('should format to string', () => {
        expect(field.formatValue('hello')).toBe('hello');
    });
});

function createField(properties: FieldPropertiesDto) {
    return new FieldDto(1, 'field1', false, false, false, 'languages', properties);
}