/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormRowComponent, TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-usage-trigger',
    styleUrls: ['./usage-trigger.component.scss'],
    templateUrl: './usage-trigger.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
    ],
})
export class UsageTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<TriggerForm>();
}
