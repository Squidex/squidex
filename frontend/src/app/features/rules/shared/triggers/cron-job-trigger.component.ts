/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppsState, CodeComponent, FormHintComponent, FormRowComponent, RulesService, TranslatePipe, TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-cron-job-trigger',
    styleUrls: ['./cron-job-trigger.component.scss'],
    templateUrl: './cron-job-trigger.component.html',
    imports: [
        CodeComponent,
        FormHintComponent,
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class CronJobTriggerComponent {
    /** The form to edit the trigger settings. */
    public readonly triggerForm = input.required<TriggerForm>();

    protected readonly timezones;

    constructor(appsState: AppsState, rulesService: RulesService) {
        this.timezones = toSignal(rulesService.getTimezones(appsState.appName), { initialValue: [] });
    }
}
