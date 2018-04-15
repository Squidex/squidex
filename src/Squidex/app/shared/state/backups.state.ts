/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DialogService,
    ImmutableArray,
    State
} from '@app/framework';

import { AppsState } from './apps.state';

import { BackupDto, BackupsService } from './../services/backups.service';

interface Snapshot {
    backups: ImmutableArray<BackupDto>;
}

@Injectable()
export class BackupsState extends State<Snapshot> {
    public backups =
        this.changes.map(x => x.backups);

    public maxBackupsReached =
        this.changes.map(x => x.backups.length >= 10);

    constructor(
        private readonly appsState: AppsState,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService
    ) {
        super({ backups: ImmutableArray.empty() });
    }

    public load(notifyLoad = false, notifyError = false): Observable<any> {
        return this.backupsService.getBackups(this.appName)
            .do(dtos => {
                if (notifyLoad) {
                    this.dialogs.notifyInfo('Backups reloaded.');
                }

                this.next(s => {
                    const backups = ImmutableArray.of(dtos);

                    return { ...s, backups };
                });
            })
            .catch(error => {
                if (notifyError) {
                    this.dialogs.notifyError(error);
                }

                return Observable.throw(error);
            });
    }

    public start(): Observable<any> {
        return this.backupsService.postBackup(this.appsState.appName)
            .do(() => {
                this.dialogs.notifyInfo('Backup started, it can take several minutes to complete.');
            })
            .notify(this.dialogs);
    }

    public delete(backup: BackupDto): Observable<any> {
        return this.backupsService.deleteBackup(this.appsState.appName, backup.id)
            .do(() => {
                this.dialogs.notifyInfo('Backup is about to be deleted.');
            })
            .notify(this.dialogs);
    }

    private get appName() {
        return this.appsState.appName;
    }
}