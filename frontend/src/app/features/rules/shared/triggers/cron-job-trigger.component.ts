/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AppsState, CodeComponent, CronJobTriggerForm, FormHintComponent, FormRowComponent, RulesService, SignalSelectDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-cron-job-trigger',
    styleUrls: ['./cron-job-trigger.component.scss'],
    templateUrl: './cron-job-trigger.component.html',
    imports: [
        CodeComponent,
        FormField,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class CronJobTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<CronJobTriggerForm>();

    protected readonly timezones = toSignal(inject(RulesService).getTimezones(inject(AppsState).appName), { initialValue: [] });
}
