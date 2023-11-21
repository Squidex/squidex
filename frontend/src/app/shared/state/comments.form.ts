/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { UntypedFormControl, Validators } from '@angular/forms';
import { ExtendedFormGroup, Form } from '@app/framework';
import { Comment } from './comments.state';

export class UpsertCommentForm extends Form<ExtendedFormGroup, Comment> {
    constructor() {
        super(new ExtendedFormGroup({
            text: new UntypedFormControl('',
                Validators.nullValidator,
            ),
        }));
    }
}
