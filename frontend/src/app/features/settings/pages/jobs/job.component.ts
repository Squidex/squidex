/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf, NgSwitch } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ApiUrlConfig, ConfirmClickDirective, Duration, ExternalLinkDirective, FromNowPipe, JobDto, JobsState, KNumberPipe, StatusIconComponent, TooltipDirective, TranslatePipe, TypedSimpleChanges } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-job',
    styleUrls: ['./job.component.scss'],
    templateUrl: './job.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        ExternalLinkDirective,
        FromNowPipe,
        KNumberPipe,
        NgIf,
        NgSwitch,
        StatusIconComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class JobComponent {
    @Input({ required: true })
    public job!: JobDto;

    public duration = '';

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly jobsState: JobsState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.job) {
            this.duration = Duration.create(this.job.started, this.job.stopped!).toString();
        }
    }

    public delete() {
        this.jobsState.delete(this.job);
    }
}
