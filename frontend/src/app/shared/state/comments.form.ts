/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form } from '@app/framework';
import { UpsertCommentDto } from './../services/comments.service';

export class UpsertCommentForm extends Form<ExtendedFormGroup, UpsertCommentDto> {
    constructor() {
        super(new ExtendedFormGroup({
            text: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}
