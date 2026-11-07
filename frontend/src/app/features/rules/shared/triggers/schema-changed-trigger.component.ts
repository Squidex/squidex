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
    selector: 'sqx-schema-changed-trigger',
    styleUrls: ['./schema-changed-trigger.component.scss'],
    templateUrl: './schema-changed-trigger.component.html',
    imports: [
        CodeComponent,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class SchemaChangedTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<TriggerForm>();
}
