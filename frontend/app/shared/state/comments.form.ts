/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, Validators } from '@angular/forms';
import { Form, UndefinableFormGroup } from '@app/framework';
import { UpsertCommentDto } from './../services/comments.service';

export class UpsertCommentForm extends Form<UndefinableFormGroup, UpsertCommentDto> {
    constructor() {
        super(new UndefinableFormGroup({
            text: new FormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}
