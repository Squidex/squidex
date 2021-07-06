/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, hasNoValue$, ValidatorsEx } from '@app/framework';
import { ClientDto, CreateClientDto, UpdateClientDto } from './../services/clients.service';

export class RenameClientForm extends Form<FormGroup, UpdateClientDto, ClientDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                ],
            ],
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
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:clients.clientIdValidationMessage'),
                ],
            ],
        }));
    }
}
