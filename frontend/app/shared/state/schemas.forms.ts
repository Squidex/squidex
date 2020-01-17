/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { map } from 'rxjs/operators';

import {
    Form,
    ValidatorsEx,
    value$
} from '@app/framework';

import {
    AddFieldDto,
    CreateSchemaDto,
    SchemaDetailsDto,
    SchemaPropertiesDto,
    SynchronizeSchemaDto,
    UpdateSchemaDto
} from './../services/schemas.service';

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
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes between.')
                ]
            ],
            isSingleton: false,
            import: {}
        }));
    }

    public transformSubmit(value: any) {
        const result = { ...value.import || {}, name: value.name, isSingleton: value.isSingleton };

        return result;
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
            noFieldRecreation: !value.fieldsDelete
        };
    }
}

type AddPreviewUrlFormType = { name: string, url: string };

export class AddPreviewUrlForm extends Form<FormGroup, AddPreviewUrlFormType> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
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
}

type ConfigurePreviewUrlsFormType = { [name: string]: string };

export class ConfigurePreviewUrlsForm extends Form<FormArray, ConfigurePreviewUrlsFormType, SchemaDetailsDto> {
    constructor(
        private readonly formBuilder: FormBuilder
    ) {
        super(formBuilder.array([]));
    }

    public add(value: any) {
        this.form.push(
            this.formBuilder.group({
                name: [value.name,
                    [
                        Validators.required
                    ]
                ],
                url: [value.url,
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
            this.add({});
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
                    ValidatorsEx.pattern('[a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)*', 'Name must be a valid javascript name in camel case.')
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