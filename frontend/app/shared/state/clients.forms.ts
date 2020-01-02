/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import {
    Form,
    hasNoValue$,
    ValidatorsEx
} from '@app/framework';

import { CreateClientDto, UpdateClientDto } from './../services/clients.service';

export class RenameClientForm extends Form<FormGroup, UpdateClientDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }
}

export class AddClientForm extends Form<FormGroup, CreateClientDto> {
    public hasNoId = hasNoValue$(this.form.controls['id']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            id: ['',
                [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes between.')
                ]
            ]
        }));
    }
}