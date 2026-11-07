/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { CodeComponent, ConditionTriggerForm, FormHintComponent, FormRowComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-asset-changed-trigger',
    styleUrls: ['./asset-changed-trigger.component.scss'],
    templateUrl: './asset-changed-trigger.component.html',
    imports: [
        CodeComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        TranslatePipe,
    ],
})
export class AssetChangedTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<ConditionTriggerForm>();
}
