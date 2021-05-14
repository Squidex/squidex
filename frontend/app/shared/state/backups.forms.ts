/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Form, hasNoValue$, ValidatorsEx } from '@app/framework';
import { StartRestoreDto } from './../services/backups.service';

export class RestoreForm extends Form<FormGroup, StartRestoreDto> {
    public hasNoUrl = hasNoValue$(this.form.controls['url']);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage'),
                ],
            ],
            url: ['',
                [
                    Validators.required,
                ],
            ],
        }));
    }
}
