/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form, TemplatedFormArray } from '@app/framework';
import { IndexDto, IndexField } from '@app/shared/internal';

export class CreateIndexForm extends Form<TemplatedFormArray, IndexField[], IndexDto> {
    public get controls(): ReadonlyArray<ExtendedFormGroup> {
        return this.form.controls as any;
    }

    constructor() {
        super(new TemplatedFormArray(FieldTemplate.INSTANCE));
    }
}

class FieldTemplate {
    public static readonly INSTANCE = new FieldTemplate();

    public createControl() {
        return new ExtendedFormGroup({
            name: new UntypedFormControl('',
                Validators.required,
            ),
            order: new UntypedFormControl('',
                Validators.required,
            ),
        });
    }
}