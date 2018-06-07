/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { map, startWith } from 'rxjs/operators';

import { Form } from '@app/framework';

export class AssignContributorForm extends Form<FormGroup> {
    public hasNoUser =
        this.form.controls['user'].valueChanges.pipe(startWith(null), map(x => !x));

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            user: [null,
                [
                    Validators.required
                ]
            ]
        }));
    }
}