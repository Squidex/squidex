/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form, ValidatorsEx } from '@app/framework';

export class CreateAppForm extends Form<FormGroup, { name: string }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes between.')
                ]
            ]
        }));
    }
}

export class UpdateAppForm extends Form<FormGroup, { label?: string, description?: string }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            label: ['',
                [
                    Validators.maxLength(40)
                ]
            ],
            description: ''
        }));
    }
}