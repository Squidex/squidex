/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, valueAll$ } from '@app/framework';
import { AppLanguageDto, UpdateAppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';

export class EditLanguageForm extends Form<FormGroup, UpdateAppLanguageDto, AppLanguageDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            isMaster: false,
            isOptional: false,
        }));

        valueAll$(this.form.controls['isMaster'])
            .subscribe(value => {
                if (value) {
                    this.form.controls['isOptional'].setValue(false);
                }
            });

        valueAll$(this.form.controls['isOptional'])
            .subscribe(value => {
                if (value) {
                    this.form.controls['isMaster'].setValue(false);
                }
            });
    }
}

type AddLanguageFormType = { language: LanguageDto };

export class AddLanguageForm extends Form<FormGroup, AddLanguageFormType> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            language: [null,
                [
                    Validators.required,
                ],
            ],
        }));
    }
}
