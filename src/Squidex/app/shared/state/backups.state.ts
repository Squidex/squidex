/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    shareSubscribed,
    State
} from '@app/framework';

import { AppsState } from './apps.state';

import { BackupDto, BackupsService } from './../services/backups.service';

interface Snapshot {
    // The current backups.
    backups: BackupsList;

    // Indicates if the backups are loaded.
    isLoaded?: boolean;
}

type BackupsList = ImmutableArray<BackupDto>;

@Injectable()
export class BackupsState extends State<Snapshot> {
    public backups =
        this.changes.pipe(map(x => x.backups),
            distinctUntilChanged());

    public maxBackupsReached =
        this.changes.pipe(map(x => x.backups.length >= 10),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService
    ) {
        super({ backups: ImmutableArray.empty() });
    }

    public load(isReload = false, silent = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.backupsService.getBackups(this.appName).pipe(
            tap(payload => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('Backups reloaded.');
                }

                this.next(s => {
                    const backups = ImmutableArray.of(payload);

                    return { ...s, backups, isLoaded: true };
                });
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public start(): Observable<any> {
        return this.backupsService.postBackup(this.appsState.appName).pipe(
            tap(() => {
                this.dialogs.notifyInfo('Backup started, it can take several minutes to complete.');
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(backup: BackupDto): Observable<any> {
        return this.backupsService.deleteBackup(this.appsState.appName, backup.id).pipe(
            tap(() => {
                this.dialogs.notifyInfo('Backup is about to be deleted.');
            }),
            shareSubscribed(this.dialogs));
    }

    private get appName() {
        return this.appsState.appName;
    }
}