/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { map, startWith } from 'rxjs/operators';

import { Form, ValidatorsEx } from '@app/framework';

const FALLBACK_NAME = 'my-app';

export class CreateAppForm extends Form<FormGroup> {
    public appName =
        this.form.controls['name'].valueChanges.pipe(map(n => n || FALLBACK_NAME), startWith(FALLBACK_NAME));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]
            ]
        }));
    }
}