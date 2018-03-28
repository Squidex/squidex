/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    AppsState,
    BackupDto,
    BackupsService,
    Duration,
    DialogService,
    ImmutableArray
} from '@app/shared';

@Component({
    selector: 'sqx-backups-page',
    styleUrls: ['./backups-page.component.scss'],
    templateUrl: './backups-page.component.html'
})
export class BackupsPageComponent implements OnInit, OnDestroy {
    private loadSubscription: Subscription;

    public backups = ImmutableArray.empty<BackupDto>();

    constructor(
        public readonly appsState: AppsState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly backupsService: BackupsService,
        private readonly dialogs: DialogService
    ) {
    }

    public ngOnDestroy() {
        this.loadSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.loadSubscription =
            Observable.timer(0, 2000)
                .switchMap(t => this.backupsService.getBackups(this.appsState.appName))
                .subscribe(dtos => {
                    this.backups = ImmutableArray.of(dtos);
                });
    }

    public startBackup() {
        this.backupsService.postBackup(this.appsState.appName)
            .subscribe(() => {
                this.dialogs.notifyInfo('Backup started, it can take several minutes to complete.');
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public deleteBackup(backup: BackupDto) {
        this.backupsService.deleteBackup(this.appsState.appName, backup.id)
            .subscribe(() => {
                this.dialogs.notifyInfo('Backup is about to be deleted.');
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public getDownloadUrl(backup: BackupDto) {
        return this.apiUrl.buildUrl(`api/apps/${this.appsState.appName}/backups/${backup.id}`);
    }

    public getDuration(backup: BackupDto) {
        return Duration.create(backup.started, backup.stopped!);
    }

    public trackByBackup(index: number, item: BackupDto) {
        return item.id;
    }
}

