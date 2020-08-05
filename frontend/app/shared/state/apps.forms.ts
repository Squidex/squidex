/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, ValidatorsEx } from '@app/framework';
import { AppDto, CreateAppDto, UpdateAppDto } from './../services/apps.service';

export class CreateAppForm extends Form<FormGroup, CreateAppDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage')
                ]
            ]
        }));
    }
}

export class UpdateAppForm extends Form<FormGroup, UpdateAppDto, AppDto> {
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