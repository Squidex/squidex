/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { map, startWith } from 'rxjs/operators';

import { Form, ValidatorsEx } from '@app/framework';

export class RenameClientForm extends Form<FormGroup> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.required
                ]
            ]
        }));
    }
}

export class AttachClientForm extends Form<FormGroup> {
    public hasNoName =
        this.form.controls['name'].valueChanges.pipe(startWith(''), map(x => !x || x.length === 0));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            name: ['',
                [
                    Validators.maxLength(40),
                    ValidatorsEx.pattern('[a-z0-9]+(\-[a-z0-9]+)*', 'Name can contain lower case letters (a-z), numbers and dashes (not at the end).')
                ]
            ]
        }));
    }
}