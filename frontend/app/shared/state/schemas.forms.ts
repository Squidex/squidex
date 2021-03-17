/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, ValidatorsEx, value$ } from '@app/framework';
import { map } from 'rxjs/operators';
import { AddFieldDto, CreateSchemaDto, FieldRule, SchemaDetailsDto, SchemaPropertiesDto, SynchronizeSchemaDto, UpdateSchemaDto } from './../services/schemas.service';
import { createProperties, FieldPropertiesDto } from './../services/schemas.types';

type CreateCategoryFormType = { name: string };

export class CreateCategoryForm extends Form<FormGroup, CreateCategoryFormType> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['']
        }));
    }
}

export class CreateSchemaForm extends Form<FormGroup, CreateSchemaDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:schemas.schemaNameValidationMessage')
                ]
            ],
            initialCategory: undefined,
            isSingleton: false,
            importing: {}
        }));
    }

    public transformLoad(value: CreateSchemaDto) {
        const { name, isSingleton, category, ...importing } = value;

        return { name, isSingleton, importing, initialCategory: category };
    }

    public transformSubmit(value: any): CreateSchemaDto {
        const { name, isSingleton, importing, initialCategory } = value;

        return { name, isSingleton, category: initialCategory, ...importing };
    }
}

export class SynchronizeSchemaForm extends Form<FormGroup, SynchronizeSchemaDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            json: {},
            fieldsDelete: false,
            fieldsRecreate: false
        }));
    }

    public loadSchema(schema: SchemaDetailsDto) {
        this.form.get('json')!.setValue(schema.export());
    }

    public transformSubmit(value: any) {
        return {
            ...value.json,
            noFieldDeletion: !value.fieldsDelete,
            noFieldRecreation: !value.fieldsRecreate
        };
    }
}

export class ConfigureFieldRulesForm extends Form<FormArray, ReadonlyArray<FieldRule>, SchemaDetailsDto> {
    constructor(
        private readonly formBuilder: FormBuilder
    ) {
        super(formBuilder.array([]));
    }

    public add(fieldNames: ReadonlyArray<string>) {
        this.form.push(
            this.formBuilder.group({
                action: ['Disable',
                    [
                        Validators.required
                    ]
                ],
                field: [fieldNames[0],
                    [
                        Validators.required
                    ]
                ],
                condition: ['',
                    [
                        Validators.required
                    ]
                ]
            }));
    }

    public remove(index: number) {
        this.form.removeAt(index);
    }

    public transformLoad(value: Partial<SchemaDetailsDto>) {
        const result = value.fieldRules || [];

        while (this.form.controls.length < result.length) {
            this.add([]);
        }

        while (this.form.controls.length > result.length) {
            this.remove(this.form.controls.length - 1);
        }

        return result;
    }
}

type ConfigurePreviewUrlsFormType = { [name: string]: string };

export class ConfigurePreviewUrlsForm extends Form<FormArray, ConfigurePreviewUrlsFormType, SchemaDetailsDto> {
    constructor(
        private readonly formBuilder: FormBuilder
    ) {
        super(formBuilder.array([]));
    }

    public add() {
        this.form.push(
            this.formBuilder.group({
                name: ['',
                    [
                        Validators.required
                    ]
                ],
                url: ['',
                    [
                        Validators.required
                    ]
                ]
            }));
    }

    public remove(index: number) {
        this.form.removeAt(index);
    }

    public transformLoad(value: Partial<SchemaDetailsDto>) {
        const result = [];

        const previewUrls = value.previewUrls || {};

        const length = Object.keys(previewUrls).length;

        while (this.form.controls.length < length) {
            this.add();
        }

        while (this.form.controls.length > length) {
            this.remove(this.form.controls.length - 1);
        }

        for (const key in previewUrls) {
            if (previewUrls.hasOwnProperty(key)) {
                result.push({ name: key, url: previewUrls[key] });
            }
        }

        return result;
    }

    public transformSubmit(value: any) {
        const result = {};

        for (const item of value) {
            result[item.name] = item.url;
        }

        return result;
    }
}

export class EditScriptsForm extends Form<FormGroup, {}, SchemaDetailsDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            query: '',
            create: '',
            change: '',
            delete: '',
            update: ''
        }));
    }
}

export class EditFieldForm extends Form<FormGroup, {}, FieldPropertiesDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(100)
                ]
            ],
            hints: ['',
                [
                    Validators.maxLength(1000)
                ]
            ],
            placeholder: ['',
                [
                    Validators.maxLength(1000)
                ]
            ],
            editorUrl: null,
            isRequired: false,
            isRequiredOnPublish: false,
            isHalfWidth: false,
            tags: []
        }));
    }
}

export class EditSchemaForm extends Form<FormGroup, UpdateSchemaDto, SchemaPropertiesDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(100)
                ]
            ],
            hints: ['',
                [
                    Validators.maxLength(1000)
                ]
            ],
            contentsSidebarUrl: '',
            contentSidebarUrl: '',
            contentEditorUrl: '',
            validateOnPublish: false,
            tags: []
        }));
    }
}

export class AddFieldForm extends Form<FormGroup, AddFieldDto> {
    public isContentField = value$(this.form.get('type')!).pipe(map(x => x !== 'UI'));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            type: ['String',
                [
                    Validators.required
                ]
            ],
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'i18n:schemas.field.nameValidationMessage')
                ]
            ],
            isLocalizable: false
        }));
    }

    public transformLoad(value: Partial<AddFieldDto>) {
        const isLocalizable = value.partitioning === 'language';

        const type =
            value.properties ?
            value.properties.fieldType :
            'String';

        return { name: value.name, isLocalizable, type };
    }

    public transformSubmit(value: any) {
        const properties = createProperties(value.type);
        const partitioning = value.isLocalizable ? 'language' : 'invariant';

        return { name: value.name, partitioning, properties };
    }
}