/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ApiUrlConfig, BackupDto, BackupsState, Duration, TypedSimpleChanges } from '@app/shared';

@Component({
    selector: 'sqx-backup',
    styleUrls: ['./backup.component.scss'],
    templateUrl: './backup.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
