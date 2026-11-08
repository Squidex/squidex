/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { CodeComponent, ConditionTriggerForm, FormHintComponent, FormRowComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-comment-trigger',
    styleUrls: ['./comment-trigger.component.scss'],
    templateUrl: './comment-trigger.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        TranslatePipe,
    ],
})
export class CommentTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<ConditionTriggerForm>();
}
