/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, hasNoValue$, UndefinableFormGroup } from '@app/framework';
import { CreateWorkflowDto } from './../services/workflows.service';

export class AddWorkflowForm extends Form<FormGroup, CreateWorkflowDto> {
    public hasNoName = hasNoValue$(this.form.controls['name']);

    constructor() {
        super(new UndefinableFormGroup({
            name: new FormControl('',
                Validators.required,
            ),
        }));
    }
}
