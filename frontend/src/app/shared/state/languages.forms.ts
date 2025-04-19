/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, value$ } from '@app/framework';
import { AddLanguageDto, AppLanguageDto, UpdateLanguageDto } from '../model';

export class EditLanguageForm extends Form<ExtendedFormGroup, UpdateLanguageDto, AppLanguageDto> {
    public get isMaster() {
        return this.form.controls['isMaster'];
    }

    public get isOptional() {
        return this.form.controls['isOptional'];
    }

    constructor() {
        super(new ExtendedFormGroup({
            isMaster: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
            isOptional: new UntypedFormControl(false,
                Validators.nullValidator,
            ),
        }));

        value$(this.isMaster)
            .subscribe(value => {
                if (value) {
                    this.isOptional.setValue(false);
                }
            });

        value$(this.isMaster)
            .subscribe(value => {
                if (value) {
                    this.isOptional.setValue(false);
                }
            });
    }

    protected transformSubmit(value: any) {
        return new UpdateLanguageDto(value);
    }
}

export class AddLanguageForm extends Form<ExtendedFormGroup, AddLanguageDto> {
    constructor() {
        super(new ExtendedFormGroup({
            language: new UntypedFormControl(null,
                Validators.required,
            ),
        }));
    }

    protected transformSubmit(value: any) {
        return new AddLanguageDto(value);
    }
}
