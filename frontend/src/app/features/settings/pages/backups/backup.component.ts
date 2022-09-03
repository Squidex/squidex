/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ApiUrlConfig, BackupDto, BackupsState, Duration } from '@app/shared';

@Component({
    selector: 'sqx-backup[backup]',
    styleUrls: ['./backup.component.scss'],
    templateUrl: './backup.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BackupComponent implements OnChanges {
    @Input()
    public backup!: BackupDto;

    public duration = '';

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly backupsState: BackupsState,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['backup']) {
            this.duration = Duration.create(this.backup.started, this.backup.stopped!).toString();
        }
    }

    public delete() {
        this.backupsState.delete(this.backup);
    }
}
