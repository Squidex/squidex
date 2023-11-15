/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CodeComponent, ControlErrorsComponent, FormHintComponent, TranslatePipe, TriggerForm } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-comment-trigger',
    styleUrls: ['./comment-trigger.component.scss'],
    templateUrl: './comment-trigger.component.html',
    imports: [
        CodeComponent,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class CommentTriggerComponent {
    @Input({ required: true })
    public triggerForm!: TriggerForm;
}
