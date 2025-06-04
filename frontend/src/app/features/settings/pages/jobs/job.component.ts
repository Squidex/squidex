/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiUrlConfig, CodeEditorComponent, ConfirmClickDirective, Duration, ExternalLinkDirective, FromNowPipe, JobDto, JobsState, StatusIconComponent, TranslatePipe, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: 'sqx-job',
    styleUrls: ['./job.component.scss'],
    templateUrl: './job.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        ExternalLinkDirective,
        FormsModule,
        FromNowPipe,
        StatusIconComponent,
        TranslatePipe,
    ]
})
export class JobComponent {
    @Input({ required: true })
    public job!: JobDto;

    public duration = '';
    public details = '';

    public isExpanded = false;

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly jobsState: JobsState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.job) {
            this.duration = Duration.create(this.job.started, this.job.stopped!).toString();

            this.details = '';
            this.details += 'Arguments:\n';
            this.details += JSON.stringify(this.job.taskArguments, undefined, 2);

            if (this.job.log.length > 0) {
                this.details += '\n\nLog:';

                for (const log of this.job.log) {
                    this.details += `\n${log.timestamp.toISODateUTC()} ${log.message}`;
                }
            }
        }
    }

    public delete() {
        this.jobsState.delete(this.job);
    }

    public toggleExpanded() {
        this.isExpanded = !this.isExpanded;
    }
}
