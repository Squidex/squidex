/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Form, hasNoValue$ } from '@app/framework';

export class AddWorkflowForm extends Form<FormGroup, { name: string}> {
    public hasNoName = hasNoValue$(this.form.controls['name']);

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