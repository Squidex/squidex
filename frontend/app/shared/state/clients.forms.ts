/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormControl, Validators } from '@angular/forms';
import { Form, hasNoValue$, UndefinableFormGroup, ValidatorsEx } from '@app/framework';
import { ClientDto, CreateClientDto, UpdateClientDto } from './../services/clients.service';

export class RenameClientForm extends Form<UndefinableFormGroup, UpdateClientDto, ClientDto> {
    constructor() {
        super(new UndefinableFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
        }));
    }
}

export class AddClientForm extends Form<UndefinableFormGroup, CreateClientDto> {
    public get id() {
        return this.form.controls['id'];
    }

    public hasNoId = hasNoValue$(this.id);

    constructor() {
        super(new UndefinableFormGroup({
            id: new FormControl('', [
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:clients.clientIdValidationMessage'),
            ]),
        }));
    }
}
