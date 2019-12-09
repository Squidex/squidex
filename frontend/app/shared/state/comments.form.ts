/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup } from '@angular/forms';

import { Form } from '@app/framework';

export class UpsertCommentForm extends Form<FormGroup, { text: string }> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            text: ''
        }));
    }
}