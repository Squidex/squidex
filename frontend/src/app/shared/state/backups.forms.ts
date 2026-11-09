/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { maxLength, required } from '@angular/forms/signals';
import { SignalForm, SignalValidators } from '@app/framework';
import { RestoreRequestDto } from '../model';

export type RestoreFormModel = { name: string; url: string };

export class RestoreForm extends SignalForm<RestoreFormModel, RestoreRequestDto> {
    public readonly hasNoUrl = computed(() => {
        return !this.model().url;
    });

    constructor() {
        super({ name: '', url: '' }, path => {
            maxLength(path.name, 40);
            SignalValidators.patternWithMessage(path.name, '[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage');
            required(path.url);
        });
    }

    protected transformSubmit(value: RestoreFormModel) {
        return new RestoreRequestDto(value);
    }
}
