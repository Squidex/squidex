/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { maxLength, required } from '@angular/forms/signals';
import { SignalForm, SignalValidators } from '@app/framework';
import { ClientDto, CreateClientDto, UpdateClientDto } from '../model';

export class RenameClientForm extends SignalForm<{ name: string }, UpdateClientDto, ClientDto> {
    constructor() {
        super({ name: '' }, path => {
            required(path.name);
        });
    }

    protected transformSubmit(value: { name: string }) {
        return new UpdateClientDto(value);
    }
}

export class AddClientForm extends SignalForm<{ id: string }, CreateClientDto> {
    public readonly hasNoId = computed(() => {
        return !this.model().id;
    });

    constructor() {
        super({ id: '' }, path => {
            maxLength(path.id, 40);
            SignalValidators.patternWithMessage(path.id, '[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:clients.clientIdValidationMessage');
        });
    }

    protected transformSubmit(value: { id: string }) {
        return new CreateClientDto(value);
    }
}
