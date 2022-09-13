/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { DialogService, LoadingState, shareSubscribed, State } from '@app/framework';
import { BackupDto, BackupsService } from './../services/backups.service';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current backups.
    backups: ReadonlyArray<BackupDto>;

    // Indicates if the user can create new backups.
    canCreate?: boolean;
}

@Injectable()
export class BackupsState extends State<Snapshot> {
    public backups =
        this.project(x => x.backups);

    public maxBackupsReached =
        this.projectFrom(this.backups, x => x.length >= 10);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService,
    ) {
        super({ backups: [] }, 'Backups');
    }

    public load(isReload = false, silent = false): Observable<any> {
        if (isReload && !silent) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload, silent);
    }

    private loadInternal(isReload: boolean, silent: boolean): Observable<any> {
        if (!silent) {
            this.next({ isLoading: true }, 'Loading Started');
        }

        return this.backupsService.getBackups(this.appName).pipe(
            tap(({ items: backups, canCreate }) => {
                if (isReload && !silent) {
                    this.dialogs.notifyInfo('i18n:backups.reloaded');
                }

                this.next({
                    backups,
                    canCreate,
                    isLoaded: true,
                    isLoading: false,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public start(): Observable<any> {
        return this.backupsService.postBackup(this.appsState.appName).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:backups.started');
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(backup: BackupDto): Observable<any> {
        return this.backupsService.deleteBackup(this.appsState.appName, backup).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:backups.deleted');
            }),
            shareSubscribed(this.dialogs));
    }
}
