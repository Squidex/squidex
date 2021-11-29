/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Form, UndefinableFormGroup } from '@app/framework';
import { UpsertCommentDto } from './../services/comments.service';

export class UpsertCommentForm extends Form<FormGroup, UpsertCommentDto> {
    constructor() {
        super(new UndefinableFormGroup({
            text: new FormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}
