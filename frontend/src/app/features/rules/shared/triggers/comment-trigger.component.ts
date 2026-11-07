/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CodeComponent, FormHintComponent, FormRowComponent, TranslatePipe, TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-comment-trigger',
    styleUrls: ['./comment-trigger.component.scss'],
    templateUrl: './comment-trigger.component.html',
    imports: [
        CodeComponent,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class CommentTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<TriggerForm>();
}
