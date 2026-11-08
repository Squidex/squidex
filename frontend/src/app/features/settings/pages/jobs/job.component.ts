/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, input, signal } from '@angular/core';
import { ApiUrlConfig, CodeEditorComponent, ConfirmClickDirective, Duration, ExternalLinkDirective, FromNowPipe, JobDto, JobsState, StatusIconComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-job',
    styleUrls: ['./job.component.scss'],
    templateUrl: './job.component.html',
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        ExternalLinkDirective,
        FromNowPipe,
        StatusIconComponent,
        TranslatePipe,
    ],
})
export class JobComponent {
    private readonly jobsState = inject(JobsState);

    protected readonly apiUrl = inject(ApiUrlConfig);

    /** The job to show. */
    public readonly job = input.required<JobDto>();

    protected readonly isExpanded = signal(false);

    protected readonly duration = computed(() => {
        const job = this.job();

        return Duration.create(job.started, job.stopped!).toString();
    });

    protected readonly details = computed(() => {
        const job = this.job();

        let details = '';
        details += 'Arguments:\n';
        details += JSON.stringify(job.taskArguments, undefined, 2);

        if (job.log.length > 0) {
            details += '\n\nLog:';

            for (const log of job.log) {
                details += `\n${log.timestamp.toISODateUTC()} ${log.message}`;
            }
        }

        return details;
    });

    protected delete() {
        this.jobsState.delete(this.job());
    }

    protected toggleExpanded() {
        this.isExpanded.update(x => !x);
    }
}
