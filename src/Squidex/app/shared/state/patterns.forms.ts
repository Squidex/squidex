/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form, ValidatorsEx } from '@app/framework';

export class EditPatternForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(100),
                    ValidatorsEx.pattern('[A-z0-9]+[A-z0-9\- ]*[A-z0-9]', 'Name can only contain letters, numbers, dashes and spaces.')
                ]
            ],
            pattern: ['',
                [
                    Validators.required
                ]
            ],
            message: ['',
                [
                    Validators.maxLength(1000)
                ]
            ]
        }));
    }
}