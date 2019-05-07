/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form, hasNoValue$ } from '@app/framework';

import { UserDto } from './../services/users.service';

export class AssignContributorForm extends Form<FormGroup, { user: string | UserDto }> {
    public hasNoUser = hasNoValue$(this.form.controls['user']);

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