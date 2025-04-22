/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, hasNoValue$, ValidatorsEx } from '@app/framework';
import { RestoreRequestDto } from '../model';

export class RestoreForm extends Form<ExtendedFormGroup, RestoreRequestDto> {
    public get url() {
        return this.form.controls['url'];
    }

    public hasNoUrl = hasNoValue$(this.url);

    constructor() {
        super(
            new ExtendedFormGroup({
                name: new UntypedFormControl('', [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage'),
                ]),
                url: new UntypedFormControl('',
                    Validators.required,
                ),
            }),
        );
    }

    protected transformSubmit(value: any) {
        return new RestoreRequestDto(value);
    }
}
