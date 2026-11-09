/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { effect } from '@angular/core';
import { required } from '@angular/forms/signals';
import { SignalForm } from '@app/framework';
import { AddLanguageDto, AppLanguageDto, UpdateLanguageDto } from '../model';

type EditLanguageFormModel = { isMaster: boolean; isOptional: boolean };

export class EditLanguageForm extends SignalForm<EditLanguageFormModel, UpdateLanguageDto, AppLanguageDto> {
    constructor() {
        super({ isMaster: false, isOptional: false });

        effect(() => {
            const model = this.model();
            if (!model.isMaster || !model.isOptional) {
                return;
            }

            this.model.set({ ...model, isOptional: false });
        });
    }

    protected transformSubmit(value: EditLanguageFormModel) {
        return new UpdateLanguageDto(value);
    }
}

export class AddLanguageForm extends SignalForm<{ language: string | null }, AddLanguageDto> {
    constructor() {
        super({ language: null }, path => {
            required(path.language);
        });
    }

    protected transformSubmit(value: { language: string | null }) {
        return new AddLanguageDto({ language: value.language! });
    }
}
