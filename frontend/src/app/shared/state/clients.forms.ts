/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, hasNoValue$, ValidatorsEx } from '@app/framework';
import { ClientDto, CreateClientDto, UpdateClientDto } from './../services/clients.service';

export class RenameClientForm extends Form<ExtendedFormGroup, UpdateClientDto, ClientDto> {
    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('',
                Validators.required,
            ),
        }));
    }
}

export class AddClientForm extends Form<ExtendedFormGroup, CreateClientDto> {
    public get id() {
        return this.form.controls['id'];
    }

    public hasNoId = hasNoValue$(this.id);

    constructor() {
        super(new ExtendedFormGroup({
            id: new UntypedFormControl('', [
                Validators.maxLength(40),
                ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:clients.clientIdValidationMessage'),
            ]),
        }));
    }
}
