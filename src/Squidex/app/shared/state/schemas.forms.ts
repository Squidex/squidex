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
    Types,
    ValidatorsEx,
    value$
} from '@app/framework';

import { AddFieldDto } from './../services/schemas.service';

import { createProperties } from './../services/schemas.types';

const FALLBACK_NAME = 'my-schema';

export class CreateCategoryForm extends Form<FormGroup, { name: string }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['']
        }));
    }
}

export class CreateSchemaForm extends Form<FormGroup, { name: string, isSingleton?: boolean, import: any }> {
    public schemaName =
        value$(this.form.controls['name']).pipe(n => n || FALLBACK_NAME);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes only (not at the end).')
                ]
            ],
            isSingleton: false,
            import: {}
        }));
    }
}

export class AddPreviewUrlForm extends Form<FormGroup, { name: string, url: string }> {
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

export class ConfigurePreviewUrlsForm extends Form<FormArray, { [name: string]: string }> {
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

    public transformLoad(value: { [name: string]: string }) {
        const result: { name: string, url: string }[] = [];

        if (Types.isObject(value)) {
            const length = Object.keys(value).length;

            while (this.form.controls.length < length) {
                this.add({});
            }

            while (this.form.controls.length > length) {
                this.remove(this.form.controls.length - 1);
            }

            for (let key in value) {
                if (value.hasOwnProperty(key)) {
                    result.push({ name: key, url: value[key] });
                }
            }
        }

        return result;
    }

    public transformSubmit(value: { name: string, url: string }[]): { [name: string]: string } {
        const result: { [name: string]: string } = {};

        for (let item of value) {
            result[item.name] = item.url;
        }

        return result;
    }
}

export class EditScriptsForm extends Form<FormGroup, { query?: string, create?: string, change?: string, delete?: string, update?: string }> {
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

export class EditFieldForm extends Form<FormGroup, { label?: string, hints?: string, placeholder?: string, editorUrl?: string, isRequired: boolean, isListField: boolean }> {
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
            isListField: false
        }));
    }
}

export class EditSchemaForm extends Form<FormGroup, { label?: string, hints?: string }> {
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
            ]
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

    public transformLoad(value: AddFieldDto) {
        const isLocalizable = value.partitioning === 'language';

        return { name: value.name, isLocalizable, type: value.properties.fieldType };
    }

    public transformSubmit(value: any): AddFieldDto {
        const properties = createProperties(value.type);
        const partitioning = value.isLocalizable ? 'language' : 'invariant';

        return { name: value.name, partitioning, properties };
    }
}