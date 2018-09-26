/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { map, startWith } from 'rxjs/operators';

import { Form, ValidatorsEx } from '@app/framework';

export class RestoreForm extends Form<FormGroup> {
    public hasNoUrl =
        this.form.controls['url'].valueChanges.pipe(startWith(null), map(x => !x));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: [null,
                [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]
            ],
            url: [null,
                [
                    Validators.required
                ]
            ]
        }));
    }
}