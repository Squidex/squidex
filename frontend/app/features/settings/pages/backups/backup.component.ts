/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ApiUrlConfig, BackupDto, BackupsState, Duration } from '@app/shared';

@Component({
    selector: 'sqx-backup[backup]',
    styleUrls: ['./backup.component.scss'],
    templateUrl: './backup.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BackupComponent {
    @Input()
    public backup: BackupDto;

    public get duration() {
        return Duration.create(this.backup.started, this.backup.stopped!).toString();
    }

    constructor(
        public readonly apiUrl: ApiUrlConfig, private readonly backupsState: BackupsState,
    ) {
    }

    public delete() {
        this.backupsState.delete(this.backup);
    }
}
