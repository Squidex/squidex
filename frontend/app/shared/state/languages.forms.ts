/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, UndefinableFormGroup, value$ } from '@app/framework';
import { AppLanguageDto, UpdateAppLanguageDto } from './../services/app-languages.service';
import { LanguageDto } from './../services/languages.service';

export class EditLanguageForm extends Form<FormGroup, UpdateAppLanguageDto, AppLanguageDto> {
    constructor() {
        super(new UndefinableFormGroup({
            isMaster: new FormControl(false,
                Validators.nullValidator,
            ),
            isOptional: new FormControl(false,
                Validators.nullValidator,
            ),
        }));

        value$(this.form.controls['isMaster'])
            .subscribe(value => {
                if (value) {
                    this.form.controls['isOptional'].setValue(false);
                }
            });

        value$(this.form.controls['isOptional'])
            .subscribe(value => {
                if (value) {
                    this.form.controls['isMaster'].setValue(false);
                }
            });
    }
}

type AddLanguageFormType = { language: LanguageDto };

export class AddLanguageForm extends Form<FormGroup, AddLanguageFormType> {
    constructor() {
        super(new UndefinableFormGroup({
            language: new FormControl(null,
                Validators.required,
            ),
        }));
    }
}
