/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form } from '@app/framework';

import { LanguageDto } from '../services/languages.service';

export class EditLanguageForm extends Form<FormGroup, { isMaster: boolean, isOptional: boolean }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            isMaster: false,
            isOptional: false
        }));

        this.form.controls['isMaster'].valueChanges
            .subscribe(value => {
                if (value) {
                    this.form.controls['isOptional'].setValue(false);
                }
            });

        this.form.controls['isOptional'].valueChanges
            .subscribe(value => {
                if (value) {
                    this.form.controls['isMaster'].setValue(false);
                }
            });
    }
}

export class AddLanguageForm extends Form<FormGroup, { language: LanguageDto }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            language: [null,
                [
                    Validators.required
                ]
            ]
        }));
    }
}