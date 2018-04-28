/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

import {
    ApiUrlConfig,
    AppsState,
    BackupDto,
    Duration
} from '@app/shared';

@Pipe({
    name: 'sqxBackupDuration',
    pure: true
})
export class BackupDurationPipe implements PipeTransform {
    public transform(backup: BackupDto) {
        return Duration.create(backup.started, backup.stopped!).toString();
    }
}

@Pipe({
    name: 'sqxBackupDownloadUrl',
    pure: true
})
export class BackupDownloadUrlPipe implements PipeTransform {
    constructor(
        private readonly apiUrl: ApiUrlConfig,
        private readonly appsState: AppsState
    ) {
    }

    public transform(backup: BackupDto) {
        return this.apiUrl.buildUrl(`api/apps/${this.appsState.appName}/backups/${backup.id}`);
    }
}