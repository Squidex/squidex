/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { maxLength, required } from '@angular/forms/signals';
import { SignalForm } from '@app/framework';
import { AddWorkflowDto } from '../model';

export interface AddWorkflowFormModel {
    name: string;
}

export class AddWorkflowForm extends SignalForm<AddWorkflowFormModel, AddWorkflowDto> {
    public readonly hasNoName = computed(() => {
        return !this.model().name;
    });

    constructor() {
        super({ name: '' }, path => {
            required(path.name);
            maxLength(path.name, 40);
        });
    }

    protected transformSubmit(value: AddWorkflowFormModel) {
        return new AddWorkflowDto(value);
    }
}
