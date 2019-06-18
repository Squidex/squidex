/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

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

    // Indicates if the user can create new backups.
    canCreate?: boolean;
}

type BackupsList = ImmutableArray<BackupDto>;

@Injectable()
export class BackupsState extends State<Snapshot> {
    public backups =
        this.project(x => x.backups);

    public maxBackupsReached =
        this.project(x => x.backups.length >= 10);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

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
            tap(({ items, canCreate }) => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('Backups reloaded.');
                }
                const backups = ImmutableArray.of(items);

                this.next(s => {

                    return { ...s, backups, isLoaded: true, canCreate };
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
        return this.backupsService.deleteBackup(this.appsState.appName, backup).pipe(
            tap(() => {
                this.dialogs.notifyInfo('Backup is about to be deleted.');
            }),
            shareSubscribed(this.dialogs));
    }

    private get appName() {
        return this.appsState.appName;
    }
}