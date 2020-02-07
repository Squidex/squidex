/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AbstractControl, FormArray } from '@angular/forms';

import {
    AppLanguageDto,
    createProperties,
    DateTime,
    EditContentForm,
    FieldDefaultValue,
    FieldFormatter,
    FieldPropertiesDto,
    FieldsValidators,
    getContentValue,
    HtmlValue,
    LanguageDto,
    MetaFields,
    NestedFieldDto,
    PartitionConfig,
    RootFieldDto,
    SchemaDetailsDto,
    SchemaPropertiesDto,
    Version
} from '@app/shared/internal';

import { TestValues } from './_test-helpers';

const {
    modified,
    modifier,
    creation,
    creator
} = TestValues;

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
    const field = createField({ properties: createProperties('Assets', { isRequired: true, minItems: 1, maxItems: 5 }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(3);
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

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
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

    it('should return default value for default properties', () => {
        const field2 = createField({ properties: createProperties('Boolean', { editor: 'Checkbox', defaultValue: true }) });

        expect(FieldDefaultValue.get(field2)).toBeTruthy();
    });
});

describe('DateTimeField', () => {
    const now = DateTime.parseISO_UTC('2017-10-12T16:30:10Z');
    const field = createField({ properties: createProperties('DateTime', { editor: 'DateTime', isRequired: true }) });

    it('should create validators', () => {
        expect(FieldsValidators.create(field, false).length).toBe(1);
    });

    it('should format to empty string if null', () => {
        expect(FieldFormatter.format(field, null)).toBe('');
    });

    it('should format to input if parsing failed', () => {
        expect(FieldFormatter.format(field, true)).toBe(true);
    });

    it('should format to date', () => {
        const dateField = createField({ properties: createProperties('DateTime', { editor: 'Date' }) });

        expect(FieldFormatter.format(dateField, '2017-12-12T16:00:00Z')).toBe('2017-12-12');
    });

    it('should format to date time', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime' }) });

        expect(FieldFormatter.format(field2, '2017-12-12T16:00:00Z')).toBe('2017-12-12 16:00:00');
    });

    it('should return default for DateFieldProperties', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime', defaultValue: '2017-10-12T16:00:00Z' }) });

        expect(FieldDefaultValue.get(field2)).toEqual('2017-10-12T16:00:00Z');
    });

    it('should return calculated date when Today for DateFieldProperties', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime', calculatedDefaultValue: 'Today' }) });

        expect(FieldDefaultValue.get(field2, now)).toEqual('2017-10-12T00:00:00Z');
    });

    it('should return calculated date when Now for DateFieldProperties', () => {
        const field2 = createField({ properties: createProperties('DateTime', { editor: 'DateTime', calculatedDefaultValue: 'Now' }) });

        expect(FieldDefaultValue.get(field2, now)).toEqual('2017-10-12T16:30:10Z');
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

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
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

    it('should return null for default properties', () => {
        expect(FieldDefaultValue.get(field)).toBeNull();
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

    it('should return default value for default properties', () => {
        const field2 = createField({ properties: createProperties('Number', { defaultValue: 13 }) });

        expect(FieldDefaultValue.get(field2)).toEqual(13);
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

    it('should return default value for default properties', () => {
        const field2 = createField({ properties: createProperties('String', { defaultValue: 'MyDefault' }) });

        expect(FieldDefaultValue.get(field2)).toEqual('MyDefault');
    });
});

describe('GetContentValue', () => {
    const language = new LanguageDto('en', 'English');
    const fieldInvariant = createField({ properties: createProperties('Number'), partitioning: 'invariant' });
    const fieldLocalized = createField({ properties: createProperties('Number') });
    const fieldAssets = createField({ properties: createProperties('Assets') });

    it('should resolve image url field from references value', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: '13'
                }
            }
        };

        const result = getContentValue(content, language, fieldAssets);

        expect(result).toEqual({ value: '13', formatted: new HtmlValue('<img src="13?width=50&height=50" />') });
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

        expect(result).toEqual({ value: '- No Value -', formatted: '- No Value -' });
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

        expect(result).toEqual({ value: '- No Value -', formatted: '- No Value -' });
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

            contentForm.arrayItemInsert(complexSchema.fields[3], languages[0]);

            const nestedForm = contentForm.form.get('field4.iv') as FormArray;
            const nestedItem = nestedForm.get([0])!;

            expect(nestedForm.controls.length).toBe(1);

            expectForm(nestedItem, 'nested41', { disabled: false, value: null });
            expectForm(nestedItem, 'nested42', { disabled: true,  value: 'Default' });
        });

        it('should remove array item', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant', nested: [
                    createNestedField({ id: 41, properties: createProperties('String') })
                ]})
            ]);

            contentForm.arrayItemInsert(complexSchema.fields[3], languages[0]);
            contentForm.arrayItemRemove(complexSchema.fields[3], languages[0], 0);

            const nestedForm = contentForm.form.get('field4.iv') as FormArray;

            expect(nestedForm.controls.length).toBe(0);
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
                for (let key in test) {
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

            simpleForm.value.subscribe(v => {
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

    function createForm(fields: RootFieldDto[]) {
        return new EditContentForm(languages,
            createSchema({ fields }));
    }
});

type SchemaValues = {
    id?: number;
    fields?: ReadonlyArray<RootFieldDto>;
    fieldsInLists?: ReadonlyArray<string>;
    fieldsInReferences?: ReadonlyArray<string>;
    properties?: SchemaPropertiesDto;
};

function createSchema({ properties, id, fields, fieldsInLists, fieldsInReferences }: SchemaValues = {}) {
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
        fieldsInReferences || []);
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