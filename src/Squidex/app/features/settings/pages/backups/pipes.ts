/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

import { BackupDto, Duration } from '@app/shared';

@Pipe({
    name: 'sqxBackupDuration',
    pure: true
})
export class BackupDurationPipe implements PipeTransform {
    public transform(backup: BackupDto) {
        return Duration.create(backup.started, backup.stopped!).toString();
    }
}