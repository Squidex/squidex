/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, hasNoValue$ } from '@app/framework';
import { AddWorkflowDto } from '../model';

export class AddWorkflowForm extends Form<ExtendedFormGroup, AddWorkflowDto> {
    public get name() {
        return this.form.controls['name'];
    }

    public hasNoName = hasNoValue$(this.name);

    constructor() {
        super(new ExtendedFormGroup({
            name: new UntypedFormControl('',
                Validators.required,
            ),
        }));
    }

    public transformSubmit(value: any) {
        return new AddWorkflowDto(value);    
    }
}
