/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { AbstractControl, FormArray } from '@angular/forms';
import { DateHelper } from '@app/framework';
import { AppLanguageDto, createProperties, DateTime, EditContentForm, FieldDefaultValue, FieldFormatter, FieldPropertiesDto, FieldsValidators, getContentValue, HtmlValue, LanguageDto, MetaFields, NestedFieldDto, RootFieldDto, SchemaDetailsDto, SchemaPropertiesDto, Version } from '@app/shared/internal';
import { FieldRule } from './../services/schemas.service';
import { FieldArrayForm } from './contents.forms';
import { PartitionConfig } from './contents.forms-helpers';
import { TestValues } from './_test-helpers';

const {
    modified,
    modifier,
    creation,
    creator
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

    it('should not return localizable if partitioning is invarient', () => {
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

describe('GetContentValue', () => {
    const language = new LanguageDto('en', 'English');
    const fieldInvariant = createField({ properties: createProperties('Number'), partitioning: 'invariant' });
    const fieldLocalized = createField({ properties: createProperties('Number') });
    const fieldAssets = createField({ properties: createProperties('Assets') });

    it('should resolve image url and filename from referenced asset', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13']
                }
            }
        };

        const assetWithImageAndFileName = createField({ properties: createProperties('Assets', { previewMode: 'ImageAndFileName' }) });

        const result = getContentValue(content, language, assetWithImageAndFileName);

        expect(result).toEqual({ value: ['url/to/13', 'file13'], formatted: new HtmlValue('<img src="url/to/13?width=50&height=50" /> <span>file13</span>') });
    });

    it('should resolve image url only from referenced asset', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13']
                }
            }
        };

        const assetWithImage = createField({ properties: createProperties('Assets', { previewMode: 'Image' }) });

        const result = getContentValue(content, language, assetWithImage);

        expect(result).toEqual({ value: ['url/to/13', 'file13'], formatted: new HtmlValue('<img src="url/to/13?width=50&height=50" />') });
    });

    it('should resolve filename only from referenced asset', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13']
                }
            }
        };

        const assetWithFileName = createField({ properties: createProperties('Assets', { previewMode: 'FileName' }) });

        const result = getContentValue(content, language, assetWithFileName);

        expect(result).toEqual({ value: ['url/to/13', 'file13'], formatted: 'file13' });
    });

    it('should resolve filename from referenced asset', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['file13']
                }
            }
        };

        const result = getContentValue(content, language, fieldAssets);

        expect(result).toEqual({ value: ['file13'], formatted: 'file13' });
    });

    it('should not image url if not found', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: null
                }
            }
        };

        const result = getContentValue(content, language, fieldAssets);

        expect(result).toEqual({ value: '-', formatted: '-' });
    });

    it('should resolve string field from references value', () => {
        const content: any = {
            referenceData: {
                field1: {
                    iv: '13'
                }
            }
        };

        const result = getContentValue(content, language, fieldInvariant);

        expect(result).toEqual({ value: '13', formatted: '13' });
    });

    it('should resolve invariant field from references value', () => {
        const content: any = {
            referenceData: {
                field1: {
                    iv: {
                        en: '13'
                    }
                }
            }
        };

        const result = getContentValue(content, language, fieldInvariant);

        expect(result).toEqual({ value: '13', formatted: '13' });
    });

    it('should resolve localized field from references value', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: {
                        en: '13'
                    }
                }
            }
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({ value: '13', formatted: '13' });
    });

    it('should return default value if reference field not found', () => {
        const content: any = {
            referenceData: {
                field1: {
                    iv: {
                        en: '13'
                    }
                }
            }
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({ value: '-', formatted: '-' });
    });

    it('should resolve invariant field', () => {
        const content: any = {
            data: {
                field1: {
                    iv: 13
                }
            }
        };

        const result = getContentValue(content, language, fieldInvariant);

        expect(result).toEqual({ value: 13, formatted: '13' });
    });

    it('should resolve localized field', () => {
        const content: any = {
            data: {
                field1: {
                    en: 13
                }
            }
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({ value: 13, formatted: '13' });
    });

    it('should return default values if field not found', () => {
        const content: any = {
            data: {
                other: {
                    en: 13
                }
            }
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({ value: undefined, formatted: '' });
    });
});

describe('ContentForm', () => {
    const languages = [
        new AppLanguageDto({}, 'en', 'English', true, false, []),
        new AppLanguageDto({}, 'de', 'English', false, true, [])
    ];

    const complexSchema = createSchema({ fields: [
        createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' }),
        createField({ id: 2, properties: createProperties('String'), isDisabled: true }),
        createField({ id: 3, properties: createProperties('String', { isRequired: true }) }),
        createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant', nested: [
            createNestedField({ id: 41, properties: createProperties('String') }),
            createNestedField({ id: 42, properties: createProperties('String', { defaultValue: 'Default' }), isDisabled: true })
        ]})
    ]});

    describe('should resolve partitions', () => {
        const partitions = new PartitionConfig(languages);

        it('should return invariant partitions', () => {
            const result = partitions.getAll(createField({ id: 3, properties: createProperties('String'), partitioning: 'invariant' }));

            expect(result).toEqual([
                { key: 'iv', isOptional: false }
            ]);
        });

        it('should return language partitions', () => {
            const result = partitions.getAll(createField({ id: 3, properties: createProperties('String') }));

            expect(result).toEqual([
                { key: 'en', isOptional: false },
                { key: 'de', isOptional: true }
            ]);
        });

        it('should return partition for language', () => {
            const result = partitions.get(languages[1]);

            expect(result).toEqual({ key: 'de', isOptional: true });
        });

        it('should return partition for no language', () => {
            const result = partitions.get();

            expect(result).toEqual({ key: 'iv', isOptional: false });
        });
    });

    describe('with complex form', () => {
        it('should not enabled disabled fields', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('String') }),
                createField({ id: 2, properties: createProperties('String'), isDisabled: true })
            ]);

            expectForm(contentForm.form, 'field1', { disabled: false });
            expectForm(contentForm.form, 'field2', { disabled: true });
        });

        it('should not create required validator for optional language', () => {
            const contentForm = createForm([
                createField({ id: 3, properties: createProperties('String', { isRequired: true }) })
            ]);

            expectForm(contentForm.form, 'field3.en', { invalid: true });
            expectForm(contentForm.form, 'field3.de', { invalid: false });
        });

        it('should require field based on condition', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('Number'), partitioning: 'invariant' }),
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' })
            ], [{
                field: 'field1', action: 'Require', condition: 'data.field2.iv < 100'
            }]);

            const field1 = contentForm.get('field1')!.get('iv');
            const field2 = contentForm.get('field2');

            expect(field1!.form.valid).toBeFalsy();

            contentForm.load({
                field2: {
                    iv: 120
                }
            });

            expect(field1!.form.valid).toBeTruthy();

            field2?.get('iv')!.form.setValue(99);

            expect(field1!.form.valid).toBeFalsy();
        });

        it('should disable field based on condition', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('Number'), partitioning: 'invariant' }),
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' })
            ], [{
                field: 'field1', action: 'Disable', condition: 'data.field2.iv > 100'
            }]);

            const field1 = contentForm.get('field1');
            const field1_iv = contentForm.get('field1')!.get('iv');

            const field2 = contentForm.get('field2');

            expect(field1!.form.disabled).toBeFalsy();

            contentForm.load({
                field1: {
                    iv: 120
                },
                field2: {
                    iv: 120
                }
            });

            expect(field1!.form.disabled).toBeTruthy();
            expect(field1_iv!.form.disabled).toBeTruthy();

            field2?.get('iv')!.form.setValue(99);

            expect(field1!.form.disabled).toBeFalsy();
            expect(field1_iv!.form.disabled).toBeFalsy();
        });

        it('should hide field based on condition', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('Number'), partitioning: 'invariant' }),
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' })
            ], [{
                field: 'field1', action: 'Hide', condition: 'data.field2.iv > 100'
            }]);

            const field1 = contentForm.get('field1');
            const field1_iv = contentForm.get('field1')!.get('iv');

            const field2 = contentForm.get('field2');

            expect(field1!.hidden).toBeFalsy();

            contentForm.load({
                field1: {
                    iv: 120
                },
                field2: {
                    iv: 120
                }
            });

            expect(field1!.hidden).toBeTruthy();
            expect(field1_iv!.hidden).toBeTruthy();

            field2?.get('iv')!.form.setValue(99);

            expect(field1!.hidden).toBeFalsy();
            expect(field1_iv!.hidden).toBeFalsy();

        });

        it('should disable nested fields based on condition', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant', nested: [
                    createNestedField({ id: 41, properties: createProperties('Number') }),
                    createNestedField({ id: 42, properties: createProperties('Number') })
                ]})
            ], [{
                field: 'field4.nested42', action: 'Disable', condition: 'itemData.nested41 > 100'
            }]);

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            contentForm.load({
                field4: {
                    iv: [{
                        nested41: 120,
                        nested42: 120
                    }, {
                        nested41: 99,
                        nested42: 99
                    }]
                }
            });

            expect(array.get(0)!.get('nested42')!.form.disabled).toBeTruthy();
            expect(array.get(1)!.get('nested42')!.form.disabled).toBeFalsy();
        });

        it('should hide nested fields based on condition', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant', nested: [
                    createNestedField({ id: 41, properties: createProperties('Number') }),
                    createNestedField({ id: 42, properties: createProperties('Number') })
                ]})
            ], [{
                field: 'field4.nested42', action: 'Hide', condition: 'itemData.nested41 > 100'
            }]);

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            contentForm.load({
                field4: {
                    iv: [{
                        nested41: 120,
                        nested42: 120
                    }, {
                        nested41: 99,
                        nested42: 99
                    }]
                }
            });

            expect(array.get(0)!.get('nested42')!.hidden).toBeTruthy();
            expect(array.get(1)!.get('nested42')!.hidden).toBeFalsy();
        });

        it('should load with array and not enable disabled nested fields', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant', nested: [
                    createNestedField({ id: 41, properties: createProperties('String') }),
                    createNestedField({ id: 42, properties: createProperties('String'), isDisabled: true })
                ]})
            ]);

            contentForm.load({
                field4: {
                    iv: [{
                        nested41: 'Text'
                    }]
                }
            });

            const nestedForm = contentForm.form.get('field4.iv') as FormArray;
            const nestedItem = nestedForm.get([0])!;

            expect(nestedForm.controls.length).toBe(1);

            expectForm(nestedItem, 'nested41', { disabled: false, value: 'Text' });
            expectForm(nestedItem, 'nested42', { disabled: true,  value: null });
        });

        it('should add array item', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant', nested: [
                    createNestedField({ id: 41, properties: createProperties('String') }),
                    createNestedField({ id: 42, properties: createProperties('String', { defaultValue: 'Default' }), isDisabled: true })
                ]})
            ]);

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            array.addItem();
            array.addItem();

            const nestedForm = contentForm.form.get('field4.iv') as FormArray;
            const nestedItem = nestedForm.get([0])!;

            expect(nestedForm.controls.length).toBe(2);

            expectForm(nestedItem, 'nested41', { disabled: false, value: null });
            expectForm(nestedItem, 'nested42', { disabled: true,  value: 'Default' });
        });

        it('should remove array item', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant', nested: [
                    createNestedField({ id: 41, properties: createProperties('String') })
                ]})
            ]);

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            array.addItem();
            array.addItem();
            array.removeItemAt(0);

            const nestedForm = contentForm.form.get('field4.iv') as FormArray;

            expect(nestedForm.controls.length).toBe(1);
        });

        it('should not array item if field has no nested fields', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant' })
            ]);
            const nestedForm = contentForm.form.get('field4.iv') as FormArray;

            expect(nestedForm.controls.length).toBe(0);
        });

        function expectForm(parent: AbstractControl, path: string, test: { invalid?: boolean, disabled?: boolean, value?: any }) {
            const form = parent.get(path);

            if (form) {
                for (const key in test) {
                    if (test.hasOwnProperty(key)) {
                        const a = form[key];
                        const e = test[key];

                        expect(a).toBe(e, `Expected ${key} of ${path} to be <${e}>, but found <${a}>.`);
                    }
                }
            } else {
                expect(form).not.toBeNull(`Expected to find form ${path}, but form not found.`);
            }
        }
    });

    it('should return true if new value is not equal to current value', () => {
        const simpleForm = createForm([
            createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' })
        ]);

        const hasChanged = simpleForm.hasChanges({ field1: { iv: 'other' }});

        expect(hasChanged).toBeTruthy();
    });

    it('should return false if new value is same as current value', () => {
        const simpleForm = createForm([
            createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' })
        ]);

        const hasChanged = simpleForm.hasChanges({ field1: { iv: null }});

        expect(hasChanged).toBeFalsy();
    });

    describe('for new content', () => {
        let simpleForm: EditContentForm;

        beforeEach(() => {
            simpleForm = createForm([
                createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' })
            ]);
        });

        it('should not be an unsaved change when nothing has changed', () => {
            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should be an unsaved change when value has changed but not saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' }});

            expect(simpleForm.hasChanged()).toBeTruthy();
        });

        it('should not be an unsaved change when value has changed and saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' }});
            simpleForm.submit();
            simpleForm.submitCompleted();

            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should subscribe to values', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' }});

            let value: any;

            simpleForm.valueChanges.subscribe(v => {
                value = v;
            });

            expect(value).toEqual({ field1: { iv: 'Change' }});
        });
    });

    describe('for editing content', () => {
        let simpleForm: EditContentForm;

        beforeEach(() => {
            simpleForm = createForm([
                createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' })
            ]);
            simpleForm.load({ field1: { iv: 'Initial' } }, true);
        });

        it('should not be an unsaved change when nothing has changed', () => {
            simpleForm.load({ field1: { iv: 'Initial' } }, true);

            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should be an unsaved change when value has changed but not saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' }});

            expect(simpleForm.hasChanged()).toBeTruthy();
        });

        it('should be an unsaved change when value has been loaded but not saved', () => {
            simpleForm.load({ field1: { iv: 'Prev' } });

            expect(simpleForm.hasChanged()).toBeTruthy();
        });

        it('should not be an unsaved change when value has changed and saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' }});
            simpleForm.submit();
            simpleForm.submitCompleted();

            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should not be an unsaved change when value has been loaded but not saved', () => {
            simpleForm.load({ field1: { iv: 'Prev' } });
            simpleForm.submit();
            simpleForm.submitCompleted();

            expect(simpleForm.hasChanged()).toBeFalsy();
        });
    });

    function createForm(fields: RootFieldDto[], fieldRules: FieldRule[] = []) {
        return new EditContentForm(languages,
            createSchema({ fields, fieldRules }), {}, 0);
    }
});

type SchemaValues = {
    id?: number;
    fields?: ReadonlyArray<RootFieldDto>;
    fieldsInLists?: ReadonlyArray<string>;
    fieldsInReferences?: ReadonlyArray<string>;
    fieldRules?: ReadonlyArray<FieldRule>;
    properties?: SchemaPropertiesDto;
};

function createSchema({ properties, id, fields, fieldsInLists, fieldsInReferences, fieldRules }: SchemaValues = {}) {
    id = id || 1;

    return new SchemaDetailsDto({},
        `schema${1}`,
        `schema${1}`,
        'category',
        properties || new SchemaPropertiesDto(), false, true,
        creation,
        creator,
        modified,
        modifier,
        new Version('1'),
        fields,
        fieldsInLists || [],
        fieldsInReferences || [],
        fieldRules || []);
}

type FieldValues = { properties: FieldPropertiesDto; id?: number; partitioning?: string; isDisabled?: boolean, nested?: ReadonlyArray<NestedFieldDto> };

function createField({ properties, id, partitioning, isDisabled, nested }: FieldValues) {
    id = id || 1;

    return new RootFieldDto({}, id, `field${id}`, properties, partitioning || 'language', false, false, isDisabled, nested);
}

function createNestedField({ properties, id, isDisabled }: FieldValues) {
    id = id || 1;

    return new NestedFieldDto({}, id, `nested${id}`, properties, 0, false, false, isDisabled);
}