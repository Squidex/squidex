/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input, ChangeDetectionStrategy } from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { FormRowComponent, UsageTriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-usage-trigger',
    styleUrls: ['./usage-trigger.component.scss'],
    templateUrl: './usage-trigger.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormRowComponent,
    ],
})
export class UsageTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<UsageTriggerForm>();
}
