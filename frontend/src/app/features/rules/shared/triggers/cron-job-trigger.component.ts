/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppsState, CodeComponent, ControlErrorsComponent, FormHintComponent, RulesService, TranslatePipe, TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-cron-job-trigger',
    styleUrls: ['./cron-job-trigger.component.scss'],
    templateUrl: './cron-job-trigger.component.html',
    imports: [
        AsyncPipe,
        CodeComponent,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class CronJobTriggerComponent {
    @Input({ required: true })
    public triggerForm!: TriggerForm;

    public timezones = this.rulesService.getTimezones(this.appsState.appName);

    constructor(
        private readonly appsState: AppsState,
        private readonly rulesService: RulesService,
    ) {
    }
}
