/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf, NgSwitch } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ApiUrlConfig, BackupDto, BackupsState, ConfirmClickDirective, Duration, ExternalLinkDirective, FromNowPipe, KNumberPipe, StatusIconComponent, TooltipDirective, TranslatePipe, TypedSimpleChanges } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-backup',
    styleUrls: ['./backup.component.scss'],
    templateUrl: './backup.component.html',
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
export class BackupComponent {
    @Input({ required: true })
    public backup!: BackupDto;

    public duration = '';

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly backupsState: BackupsState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.backup) {
            this.duration = Duration.create(this.backup.started, this.backup.stopped!).toString();
        }
    }

    public delete() {
        this.backupsState.delete(this.backup);
    }
}
