/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ControlErrorsComponent, FormHintComponent, TranslatePipe, TriggerForm } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-usage-trigger',
    styleUrls: ['./usage-trigger.component.scss'],
    templateUrl: './usage-trigger.component.html',
    imports: [
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class UsageTriggerComponent {
    @Input({ required: true })
    public triggerForm!: TriggerForm;
}
