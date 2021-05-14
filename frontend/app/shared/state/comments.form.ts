/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormBuilder, FormGroup } from '@angular/forms';
import { Form } from '@app/framework';
import { UpsertCommentDto } from './../services/comments.service';

export class UpsertCommentForm extends Form<FormGroup, UpsertCommentDto> {
    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            text: '',
        }));
    }
}
