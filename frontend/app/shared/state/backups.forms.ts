/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { FormControl, Validators } from '@angular/forms';
import { Form, hasNoValue$, UndefinableFormGroup, ValidatorsEx } from '@app/framework';
import { StartRestoreDto } from './../services/backups.service';

export class RestoreForm extends Form<UndefinableFormGroup, StartRestoreDto> {
    public get url() {
        return this.form.controls['url'];
    }

    public hasNoUrl = hasNoValue$(this.url);

    constructor() {
        super(
            new UndefinableFormGroup({
                name: new FormControl('', [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage'),
                ]),
                url: new FormControl('',
                    Validators.required,
                ),
            }),
        );
    }
}
