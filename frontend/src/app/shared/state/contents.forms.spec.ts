/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/naming-convention */

import { AbstractControl, UntypedFormArray } from '@angular/forms';
import { MathHelper } from '@app/framework';
import { AppLanguageDto, createProperties, EditContentForm, FieldDto, FieldRuleDto, getContentValue, HtmlValue, SchemaDto } from '@app/shared/internal';
import { TestValues } from './_test-helpers';
import { ComponentForm, FieldArrayForm } from './contents.forms';
import { contentsTranslationStatus, contentTranslationStatus, fieldTranslationStatus, PartitionConfig } from './contents.forms-helpers';

const {
    createField,
    createNestedField,
    createSchema,
} = TestValues;

describe('TranslationStatus', () => {
    const languages = [
        { iso2Code: 'en' },
        { iso2Code: 'de' },
        { iso2Code: 'it' },
    ];

    it('should create field status', () => {
        const data = {
            en: '',
            de: 'field2',
            it: true,
            es: null,
        };

        const result = fieldTranslationStatus(data);

        expect(result).toEqual({
            en: true,
            de: true,
            it: true,
            es: false,
        });
    });

    it('should create content status for empty schema', () => {
        const schema = {
            fields: [],
        } as any;

        const result = contentTranslationStatus({}, schema, languages as any);

        expect(result).toEqual({
            en: 100,
            de: 100,
            it: 100,
        });
    });

    it('should create content status for schema without localized field', () => {
        const schema = {
            fields: [{
                isLocalizable: false,
            }],
        } as any;

        const result = contentTranslationStatus({}, schema, languages as any);

        expect(result).toEqual({
            en: 100,
            de: 100,
            it: 100,
        });
    });

    it('should create content status for schema with localized field', () => {
        const schema = {
            fields: [{
                isLocalizable: true,
            }],
        } as any;

        const result = contentTranslationStatus({}, schema, languages as any);

        expect(result).toEqual({
            en: 0,
            de: 0,
            it: 0,
        });
    });

    it('should create content status for schema with mixed fields', () => {
        const schema = {
            fields: [{
                name: 'field1', isLocalizable: true,
            }, {
                name: 'field2', isLocalizable: true,
            }, {
                name: 'field3', isLocalizable: true,
            }, {
                name: 'field4',
            }],
        } as any;

        const data = {
            field1: {
                en: 'en',
                de: 'de',
            },
            field2: {
                en: 'en',
                de: 'de',
            },
            field3: {
                en: 'en',
            },
        };

        const result = contentTranslationStatus(data, schema, languages as any);

        expect(result).toEqual({
            en: 100,
            de: 67,
            it: 0,
        });
    });

    it('should create contents status', () => {
        const schema = {
            fields: [{
                name: 'field1', isLocalizable: true,
            }, {
                name: 'field2', isLocalizable: true,
            }, {
                name: 'field3', isLocalizable: true,
            }, {
                name: 'field4',
            }],
        } as any;

        const data1 = {
            field1: {
                en: 'en',
                de: 'de',
            },
            field2: {
                en: 'en',
                de: 'de',
            },
            field3: {
                en: 'en',
            },
        };

        const data2 = {
            field1: {
                de: 'de',
            },
            field3: {
                en: 'en',
            },
        };

        const result = contentsTranslationStatus([data1, data2], schema, languages as any);

        expect(result).toEqual({
            en: 67,
            de: 50,
            it: 0,
        });
    });
});

describe('GetContentValue', () => {
    const language = new AppLanguageDto({
        iso2Code: 'en',
        englishName: 'English',
        isMaster: false,
        isOptional: false,
        fallback: [],
        _links: {},
    });

    const fieldInvariant = createField({ properties: createProperties('Number'), partitioning: 'invariant' });
    const fieldLocalized = createField({ properties: createProperties('Number') });
    const fieldAssets = createField({ properties: createProperties('Assets') });

    it('should resolve formatted text as image url and filename from referenced asset', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13'],
                },
            },
        };

        const assetWithImageAndFileName = createField({ properties: createProperties('Assets', { previewMode: 'ImageAndFileName' }) });

        const result = getContentValue(content, language, assetWithImageAndFileName);

        expect(result).toEqual({
            value: undefined,
            formatted: new HtmlValue('<div class="image"><img src="url/to/13?width=50&height=50&mode=Pad&" /> <span>file13</span></div>', 'url/to/13'),
        });
    });

    it('should resolve formatted text as image url from referenced asset', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13'],
                },
            },
        };

        const assetWithImage = createField({ properties: createProperties('Assets', { previewMode: 'Image' }) });

        const result = getContentValue(content, language, assetWithImage);

        expect(result).toEqual({
            value: undefined,
            formatted: new HtmlValue('<div class="image"><img src="url/to/13?width=50&height=50&mode=Pad&" /></div>', 'url/to/13'),
        });
    });

    it('should resolve formatted text as image url from referenced asset with custom format', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13'],
                },
            },
        };

        const assetWithImage = createField({ properties: createProperties('Assets', { previewMode: 'Image', previewFormat: 'width=100&height=100' }) });

        const result = getContentValue(content, language, assetWithImage);

        expect(result).toEqual({
            value: undefined,
            formatted: new HtmlValue('<div class="image"><img src="url/to/13?width=100&height=100" /></div>', 'url/to/13'),
        });
    });

    it('should resolve formatted text as image url from referenced asset with merged format', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13'],
                },
            },
        };

        const assetWithImage = createField({ properties: createProperties('Assets', { previewMode: 'Image', previewFormat: 'bg=red' }) });

        const result = getContentValue(content, language, assetWithImage);

        expect(result).toEqual({
            value: undefined,
            formatted: new HtmlValue('<div class="image"><img src="url/to/13?width=50&height=50&mode=Pad&bg=red" /></div>', 'url/to/13'),
        });
    });

    it('should resolve formatted text as filename from referenced asset', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['url/to/13', 'file13'],
                },
            },
        };

        const assetWithFileName = createField({ properties: createProperties('Assets', { previewMode: 'FileName' }) });

        const result = getContentValue(content, language, assetWithFileName);

        expect(result).toEqual({
            value: undefined,
            formatted: 'file13',
        });
    });

    it('should resolve formatted text as filename from referenced asset with fallback format', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: ['file13'],
                },
            },
        };

        const result = getContentValue(content, language, fieldAssets);

        expect(result).toEqual({
            value: undefined,
            formatted: 'file13',
        });
    });

    it('should not resolve formatted text as image url if not found', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: null,
                },
            },
        };

        const result = getContentValue(content, language, fieldAssets);

        expect(result).toEqual({
            value: undefined,
            formatted: '',
        });
    });

    it('should resolve formatted text from invariant reference data', () => {
        const content: any = {
            referenceData: {
                field1: {
                    iv: '42',
                },
            },
        };

        const result = getContentValue(content, language, fieldInvariant);

        expect(result).toEqual({
            value: undefined,
            formatted: '42',
        });
    });

    it('should resolve formatted text from localized-invariant reference data', () => {
        const content: any = {
            referenceData: {
                field1: {
                    iv: {
                        en: '42',
                    },
                },
            },
        };

        const result = getContentValue(content, language, fieldInvariant);

        expect(result).toEqual({
            value: undefined,
            formatted: '42',
        });
    });

    it('should resolve formatted text from localized-localized reference data', () => {
        const content: any = {
            referenceData: {
                field1: {
                    en: {
                        en: '13',
                    },
                },
            },
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({ value: undefined, formatted: '13' });
    });

    it('should not resolve formatted text if reference data not found', () => {
        const content: any = {
            referenceData: {
                field1: {
                    iv: {
                        en: '13',
                    },
                },
            },
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({
            value: undefined,
            formatted: '',
        });
    });

    it('should resolve value from invariant field', () => {
        const content: any = {
            data: {
                field1: {
                    iv: 13,
                },
            },
        };

        const result = getContentValue(content, language, fieldInvariant);

        expect(result).toEqual({
            value: 13,
            formatted: '13',
        });
    });

    it('should resolve value from invariant field as zero', () => {
        const content: any = {
            data: {
                field1: {
                    iv: 0,
                },
            },
        };

        const result = getContentValue(content, language, fieldInvariant);

        expect(result).toEqual({
            value: 0,
            formatted: '0',
        });
    });

    it('should resolve value from localized field', () => {
        const content: any = {
            data: {
                field1: {
                    en: 13,
                },
            },
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({
            value: 13,
            formatted: '13',
        });
    });

    it('should not resolve value if field not found', () => {
        const content: any = {
            data: {
                other: {
                    en: 13,
                },
            },
        };

        const result = getContentValue(content, language, fieldLocalized);

        expect(result).toEqual({
            value: undefined,
            formatted: '',
        });
    });
});

describe('ContentForm', () => {
    const languages = [
        new AppLanguageDto({
            iso2Code: 'en',
            englishName: 'English',
            isMaster: true,
            isOptional: false,
            fallback: [],
            _links: {},
        }),
        new AppLanguageDto({
            iso2Code: 'de',
            englishName: 'Getman',
            isMaster: false,
            isOptional: true,
            fallback: [],
            _links: {},
        }),
    ];

    const complexSchema = createSchema({ fields: [
        createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' }),
        createField({ id: 2, properties: createProperties('String'), isDisabled: true }),
        createField({ id: 3, properties: createProperties('String', { isRequired: true }) }),
        createField({
            id: 4,
            properties: createProperties('Array'),
            nested: [
                createNestedField({ id: 41, properties: createProperties('String') }),
                createNestedField({ id: 42, properties: createProperties('String', { defaultValue: 'Default' }), isDisabled: true }),
            ],
            partitioning: 'invariant',
        }),
    ] });

    describe('should resolve partitions', () => {
        const partitions = new PartitionConfig(languages);

        it('should return invariant partitions', () => {
            const result = partitions.getAll(createField({ id: 3, properties: createProperties('String'), partitioning: 'invariant' }));

            expect(result).toEqual([
                { key: 'iv', isOptional: false },
            ]);
        });

        it('should return language partitions', () => {
            const result = partitions.getAll(createField({ id: 3, properties: createProperties('String') }));

            expect(result).toEqual([
                { key: 'en', isOptional: false },
                { key: 'de', isOptional: true },
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
        const arraySetup = [
            {
                value: [],
                defaultValue: 'EmptyArray',

            },
            {
                value: undefined,
                defaultValue: 'Null',
            },
        ];

        it('should not enabled disabled fields', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('String') }),
                createField({ id: 2, properties: createProperties('String'), isDisabled: true }),
            ]);

            expectForm(contentForm.form, 'field1', { disabled: false });
            expectForm(contentForm.form, 'field2', { disabled: true });
        });

        it('should not create required validator for optional language', () => {
            const contentForm = createForm([
                createField({ id: 3, properties: createProperties('String', { isRequired: true }) }),
            ]);

            expectForm(contentForm.form, 'field3.en', { invalid: true });
            expectForm(contentForm.form, 'field3.de', { invalid: false });
        });

        arraySetup.forEach(test => {
            it(`should create array with ${test.defaultValue} default value`, () => {
                const contentForm = createForm([
                    createField({
                        id: 4,
                        properties: createProperties('Array', { calculatedDefaultValue: test.defaultValue }),
                    }),
                ]);

                expect(contentForm.value.field4.en).toEqual(test.value);
            });
        });

        arraySetup.forEach(test => {
            it(`should create components with ${test.defaultValue} default value`, () => {
                const contentForm = createForm([
                    createField({
                        id: 4,
                        properties: createProperties('Components', { calculatedDefaultValue: test.defaultValue }),
                    }),
                ]);

                expect(contentForm.value.field4.en).toEqual(test.value);
            });
        });

        it('should require field based on context condition', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('Number'), partitioning: 'invariant' }),
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' }),
            ], [
                new FieldRuleDto({ field: 'field1', action: 'Require', condition: 'ctx.value < 100' }),
            ]);

            contentForm.setContext({ value: 50 });

            const field1 = contentForm.get('field1')!.get('iv');

            expect(field1!.form.valid).toBeFalsy();

            contentForm.setContext({ value: 120 });

            expect(field1!.form.valid).toBeTruthy();
        });

        it('should require field based on condition', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('Number'), partitioning: 'invariant' }),
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' }),
            ], [
                new FieldRuleDto({ field: 'field1', action: 'Require', condition: 'data.field2.iv < 100' }),
            ]);

            const field1 = contentForm.get('field1')!.get('iv');
            const field2 = contentForm.get('field2');

            expect(field1!.form.valid).toBeFalsy();

            contentForm.load({
                field2: {
                    iv: 120,
                },
            });

            expect(field1!.form.valid).toBeTruthy();

            field2?.get('iv')!.form.setValue(99);

            expect(field1!.form.valid).toBeFalsy();
        });

        it('should disable field based on condition', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('Number'), partitioning: 'invariant' }),
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' }),
            ], [
                new FieldRuleDto({ field: 'field1', action: 'Disable', condition: 'data.field2.iv > 100' }),
            ]);

            const field1 = contentForm.get('field1');
            const field1_iv = contentForm.get('field1')!.get('iv');

            const field2 = contentForm.get('field2');

            expect(field1!.form.disabled).toBeFalsy();

            contentForm.load({
                field1: {
                    iv: 120,
                },
                field2: {
                    iv: 120,
                },
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
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' }),
            ], [
                new FieldRuleDto({ field: 'field1', action: 'Hide', condition: 'data.field2.iv > 100' }),
            ]);

            const field1 = contentForm.get('field1');
            const field1_iv = contentForm.get('field1')!.get('iv');

            const field2 = contentForm.get('field2');

            expect(field1!.hidden).toBeFalsy();

            contentForm.load({
                field1: {
                    iv: 120,
                },
                field2: {
                    iv: 120,
                },
            });

            expect(field1!.hidden).toBeTruthy();
            expect(field1_iv!.hidden).toBeTruthy();

            field2?.get('iv')!.form.setValue(99);

            expect(field1!.hidden).toBeFalsy();
            expect(field1_iv!.hidden).toBeFalsy();
        });

        it('should disable nested fields based on condition', () => {
            const contentForm = createForm([
                createField({
                    id: 4,
                    properties: createProperties('Array'),
                    nested: [
                        createNestedField({ id: 41, properties: createProperties('Number') }),
                        createNestedField({ id: 42, properties: createProperties('Number') }),
                    ],
                    partitioning: 'invariant',
                }),
            ], [
                new FieldRuleDto({ field: 'field4.nested42', action: 'Disable', condition: 'itemData.nested41 > 100' }),
            ]);

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            contentForm.load({
                field4: {
                    iv: [{
                        nested41: 120,
                        nested42: 120,
                    }, {
                        nested41: 99,
                        nested42: 99,
                    }],
                },
            });

            expect(array.get(0)!.get('nested42')!.form.disabled).toBeTruthy();
            expect(array.get(1)!.get('nested42')!.form.disabled).toBeFalsy();
        });

        it('should hide nested fields based on condition', () => {
            const contentForm = createForm([
                createField({
                    id: 4,
                    properties: createProperties('Array'),
                    nested: [
                        createNestedField({ id: 41, properties: createProperties('Number') }),
                        createNestedField({ id: 42, properties: createProperties('Number') }),
                    ],
                    partitioning: 'invariant',
                }),
            ], [
                new FieldRuleDto({ field: 'field4.nested42', action: 'Hide', condition: 'itemData.nested41 > 100' }),
            ]);

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            contentForm.load({
                field4: {
                    iv: [{
                        nested41: 120,
                        nested42: 120,
                    }, {
                        nested41: 99,
                        nested42: 99,
                    }],
                },
            });

            expect(array.get(0)!.get('nested42')!.hidden).toBeTruthy();
            expect(array.get(1)!.get('nested42')!.hidden).toBeFalsy();
        });

        it('should hide nested localized fields based on condition', () => {
            const contentForm = createForm([
                createField({
                    id: 4,
                    properties: createProperties('Array'),
                    nested: [
                        createNestedField({ id: 41, properties: createProperties('Number') }),
                        createNestedField({ id: 42, properties: createProperties('Number') }),
                    ],
                    partitioning: 'language',
                }),
            ], [
                new FieldRuleDto({ field: 'field4.nested42', action: 'Hide', condition: 'itemData.nested41 > 100' }),
            ]);

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            contentForm.load({
                field4: {
                    en: [{
                        nested41: 120,
                        nested42: 120,
                    }, {
                        nested41: 99,
                        nested42: 99,
                    }],
                },
            });

            expect(array.get(0)!.get('nested42')!.hidden).toBeTruthy();
            expect(array.get(1)!.get('nested42')!.hidden).toBeFalsy();
        });

        it('should hide components fields based on condition', () => {
            const componentId = MathHelper.guid();
            const component = createSchema({
                id: 2,
                fields: [
                    createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' }),
                ],
                fieldRules: [
                    new FieldRuleDto({ field: 'field1', action: 'Hide', condition: 'data.field1 > 100' }),
                ],
            });

            const contentForm = createForm([
                createField({
                    id: 4,
                    properties: createProperties('Components'),
                    partitioning: 'invariant',
                }),
            ], [], {
                [componentId]: component,
            });

            const array = contentForm.get(complexSchema.fields[3])!.get(languages[0]) as FieldArrayForm;

            contentForm.load({
                field4: {
                    iv: [{
                        schemaId: componentId,
                        field1: 120,
                    }, {
                        schemaId: componentId,
                        field1: 99,
                    }],
                },
            });

            expect(array.get(0)!.get('field1')!.hidden).toBeTruthy();
            expect(array.get(1)!.get('field1')!.hidden).toBeFalsy();
        });

        it('should hide field based on tags', () => {
            const contentForm = createForm([
                createField({ id: 1, properties: createProperties('Number', { tags: ['tag1'] }), partitioning: 'invariant' }),
                createField({ id: 2, properties: createProperties('Number'), partitioning: 'invariant' }),
            ], [
                new FieldRuleDto({ field: 'tag:tag1', action: 'Hide' }),
            ]);

            const field1 = contentForm.get('field1');
            const field1_iv = contentForm.get('field1')!.get('iv');
            const field2 = contentForm.get('field2');

            expect(field1!.hidden).toBeTruthy();
            expect(field2!.hidden).toBeFalsy();
            expect(field1_iv!.hidden).toBeTruthy();
        });

        it('should add component with default values', () => {
            const componentId = MathHelper.guid();
            const component = createSchema({
                id: 1,
                fields: [
                    createField({
                        id: 11,
                        properties: createProperties('String', {
                            defaultValue: 'Initial',
                        }),
                        partitioning: 'invariant',
                    }),
                    createField({
                        id: 12,
                        properties: createProperties('Number', {
                            defaultValue: 12,
                        }),
                        partitioning: 'invariant',
                    }),
                ],
            });

            const contentForm = createForm([
                createField({
                    id: 4,
                    properties: createProperties('Component'),
                    partitioning: 'invariant',
                }),
            ], [], {
                [componentId]: component,
            });

            contentForm.load({});

            // Should be undefined by default.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: undefined,
                },
            });

            (contentForm.get('field4')?.get('iv') as ComponentForm).selectSchema(componentId);

            // Should add field from component.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: {
                        schemaId: componentId,
                        field11: 'Initial',
                        field12: 12,
                    },
                },
            });
        });

        it('should add components with default values', () => {
            const componentId = MathHelper.guid();
            const component = createSchema({
                id: 1,
                fields: [
                    createField({
                        id: 11,
                        properties: createProperties('String', {
                            defaultValue: 'Initial',
                        }),
                        partitioning: 'invariant',
                    }),
                    createField({
                        id: 12,
                        properties: createProperties('Number', {
                            defaultValue: 12,
                        }),
                        partitioning: 'invariant',
                    }),
                ],
            });

            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Components'), partitioning: 'invariant' }),
            ], [], {
                [componentId]: component,
            });

            contentForm.load({});

            // Should be undefined by default.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: undefined,
                },
            });

            (contentForm.get('field4')?.get('iv') as FieldArrayForm).addComponent(componentId);

            // Should add field from component.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: [{
                        schemaId: componentId,
                        field11: 'Initial',
                        field12: 12,
                    }],
                },
            });
        });

        it('should replace component with new fields', () => {
            const component1Id = MathHelper.guid();
            const component1 = createSchema({
                id: 1,
                fields: [
                    createField({ id: 11, properties: createProperties('String'), partitioning: 'invariant' }),
                ],
            });

            const component2Id = MathHelper.guid();
            const component2 = createSchema({
                id: 2,
                fields: [
                    createField({ id: 21, properties: createProperties('String'), partitioning: 'invariant' }),
                ],
            });

            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Component'), partitioning: 'invariant' }),
            ], [], {
                [component1Id]: component1,
                [component2Id]: component2,
            });

            contentForm.load({});

            // Should be undefined by default.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: undefined,
                },
            });

            contentForm.load({
                field4: {
                    iv: {
                        schemaId: component1Id,
                    },
                },
            });

            // Should add field from component1.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: {
                        schemaId: component1Id,
                        field11: null,
                    },
                },
            });

            contentForm.load({
                field4: {
                    iv: {
                        schemaId: component2Id,
                    },
                },
            });

            // Should add field from component1.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: {
                        schemaId: component2Id,
                        field21: null,
                    },
                },
            });
        });

        it('should ignore invalid schema ids', () => {
            const componentId = MathHelper.guid();
            const component = createSchema({
                id: 1,
                fields: [
                    createField({ id: 11, properties: createProperties('String'), partitioning: 'invariant' })],
            });

            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Component'), partitioning: 'invariant' }),
            ], [], {
                [componentId]: component,
            });

            contentForm.load({
                field4: {
                    iv: {
                        schemaId: 'invalid',
                    },
                },
            });

            // Should ignore invalid id.
            expect(contentForm.value).toEqual({
                field4: {
                    iv: {},
                },
            });
        });

        it('should load with array and not enable disabled nested fields', () => {
            const { contentForm, array } = createArrayFormWith2Items();

            contentForm.load({
                field4: {
                    iv: [{
                        nested41: 'Text',
                    }],
                },
            });

            const nestedItem = array.form.get([0])!;

            expectLength(array, 1);
            expectForm(nestedItem, 'nested41', { disabled: false, value: 'Text' });
            expectForm(nestedItem, 'nested42', { disabled: true, value: null });
        });

        it('should add array item', () => {
            const { array } = createArrayFormWith2Items();

            array.addItem();

            const nestedItem = array.form.get([2])!;

            expectLength(array, 3);
            expectForm(nestedItem, 'nested41', { disabled: false, value: null });
            expectForm(nestedItem, 'nested42', { disabled: true, value: 'Default' });
        });

        it('should sort array item', () => {
            const { array } = createArrayFormWith2Items();

            array.sort([array.get(1), array.get(0)]);

            expectLength(array, 2);
            expect(array.form.value).toEqual([{ nested41: 'Text2', nested42: null }, { nested41: 'Text1', nested42: null }]);
        });

        it('should remove array item', () => {
            const { array } = createArrayFormWith2Items();

            array.removeItemAt(0);

            expectLength(array, 1);
            expect(array.form.value).toEqual([{ nested41: 'Text2', nested42: null }]);
        });

        it('should reset array item', () => {
            const { array } = createArrayFormWith2Items();

            array.setValue([]);

            expectLength(array, 0);
            expect(array.form.value).toEqual([]);
        });

        it('should unset array item', () => {
            const { array } = createArrayFormWith2Items();

            array.unset();

            expectLength(array, 0);
            expect(array.form.value).toEqual(undefined);
        });

        it('should not array item if field has no nested fields', () => {
            const contentForm = createForm([
                createField({ id: 4, properties: createProperties('Array'), partitioning: 'invariant' }),
            ]);

            const nestedForm = contentForm.form.get('field4.iv') as UntypedFormArray;

            expect(nestedForm.controls.length).toBe(0);
        });

        function createArrayFormWith2Items() {
            const contentForm = createForm([
                createField({
                    id: 4,
                    properties: createProperties('Array'),
                    partitioning: 'invariant',
                    nested: [
                        createNestedField({ id: 41, properties: createProperties('String') }),
                        createNestedField({ id: 42, properties: createProperties('String', { defaultValue: 'Default' }), isDisabled: true }),
                    ],
                }),
            ]);

            const array = contentForm.get('field4')!.get('iv') as FieldArrayForm;

            contentForm.load({
                field4: {
                    iv: [{
                        nested41: 'Text1',
                    }, {
                        nested41: 'Text2',
                    }],
                },
            });

            return { contentForm, array };
        }

        function expectLength(array: FieldArrayForm, length: number) {
            expect(array.form.controls.length).toBe(length);
            expect(array.items.length).toBe(length);
        }

        function expectForm(parent: AbstractControl, path: string, test: { invalid?: boolean; disabled?: boolean; value?: any } & Record<string, any>) {
            const form = parent.get(path) as Record<string, any>;

            if (form) {
                for (const key in test) {
                    if (test.hasOwnProperty(key)) {
                        const valueActual = form[key];
                        const valueExpected = test[key];

                        expect(valueActual).withContext(`Expected ${key} of ${path} to be <${valueExpected}>, but found <${valueActual}>.`).toEqual(valueExpected);
                    }
                }
            } else {
                expect(form).withContext(`Expected to find form ${path}, but form not found.`).not.toBeNull();
            }
        }
    });

    it('should return true if new value is not equal to current value', () => {
        const simpleForm = createForm([
            createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' }),
        ]);

        const hasChanged = simpleForm.hasChanges({ field1: { iv: 'other' } });

        expect(hasChanged).toBeTruthy();
    });

    it('should return false if new value is same as current value', () => {
        const simpleForm = createForm([
            createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' }),
        ]);

        const hasChanged = simpleForm.hasChanges({ field1: { iv: null } });

        expect(hasChanged).toBeFalsy();
    });

    describe('for new content', () => {
        let simpleForm: EditContentForm;

        beforeEach(() => {
            simpleForm = createForm([
                createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' }),
            ]);
        });

        it('should not be an unsaved change if nothing has changed', () => {
            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should be an unsaved change if value has changed but not saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' } });

            expect(simpleForm.hasChanged()).toBeTruthy();
        });

        it('should not be an unsaved change if value has changed and saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' } });
            simpleForm.submit();
            simpleForm.submitCompleted();

            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should subscribe to values', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' } });

            let value: any;

            simpleForm.valueChanges.subscribe(result => {
                value = result;
            });

            expect(value).toEqual({ field1: { iv: 'Change' } });
        });
    });

    describe('for editing content', () => {
        let simpleForm: EditContentForm;

        beforeEach(() => {
            simpleForm = createForm([
                createField({ id: 1, properties: createProperties('String'), partitioning: 'invariant' }),
            ]);
            simpleForm.load({ field1: { iv: 'Initial' } }, true);
        });

        it('should not be an unsaved change if nothing has changed', () => {
            simpleForm.load({ field1: { iv: 'Initial' } }, true);

            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should be an unsaved change if value has changed but not saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' } });

            expect(simpleForm.hasChanged()).toBeTruthy();
        });

        it('should be an unsaved change if value has been loaded but not saved', () => {
            simpleForm.load({ field1: { iv: 'Prev' } });

            expect(simpleForm.hasChanged()).toBeTruthy();
        });

        it('should not be an unsaved change if value has changed and saved', () => {
            simpleForm.form.setValue({ field1: { iv: 'Change' } });
            simpleForm.submit();
            simpleForm.submitCompleted();

            expect(simpleForm.hasChanged()).toBeFalsy();
        });

        it('should not be an unsaved change if value has been loaded but not saved', () => {
            simpleForm.load({ field1: { iv: 'Prev' } });
            simpleForm.submit();
            simpleForm.submitCompleted();

            expect(simpleForm.hasChanged()).toBeFalsy();
        });
    });

    function createForm(fields: FieldDto[], fieldRules: FieldRuleDto[] = [], schemas: { [id: string]: SchemaDto } = {}) {
        return new EditContentForm(languages,
            createSchema({ fields, fieldRules }), schemas, {}, 0);
    }
});
