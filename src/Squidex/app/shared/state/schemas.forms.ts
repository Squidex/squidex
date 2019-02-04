/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    Form,
    Types,
    ValidatorsEx,
    value$
} from '@app/framework';

import { createProperties } from './../services/schemas.types';

const FALLBACK_NAME = 'my-schema';

export class CreateCategoryForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['']
        }));
    }
}

export class CreateSchemaForm extends Form<FormGroup> {
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

export class AddPreviewUrlForm extends Form<FormGroup> {
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

export class ConfigurePreviewUrlsForm extends Form<FormArray> {
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

    public load(value?: any) {
        if (Types.isObject(value)) {
            const length = Object.keys(value).length;

            while (this.form.controls.length < length) {
                this.add({});
            }

            while (this.form.controls.length > length) {
                this.remove(this.form.controls.length - 1);
            }

            const array: any[] = [];

            for (let key in value) {
                if (value.hasOwnProperty(key)) {
                    array.push({ name: key, url: value[key] });
                }
            }

            value = array;
        }

        super.load(value);
    }

    public submit() {
        let result = super.submit();

        if (result) {
            const hash: { [name: string]: string } = {};

            for (let item of result) {
                hash[item.name] = item.url;
            }

            result = hash;
        }

        return result;
    }
}

export class EditScriptsForm extends Form<FormGroup> {
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

export class EditFieldForm extends Form<FormGroup> {
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

export class EditSchemaForm extends Form<FormGroup> {
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

export class AddFieldForm extends Form<FormGroup> {
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

    public submit() {
        const value = super.submit();

        if (value) {
            const properties = createProperties(value.type);
            const partitioning = value.isLocalizable ? 'language' : 'invariant';

            return { name: value.name, partitioning, properties };
        }

        return null;
    }
}