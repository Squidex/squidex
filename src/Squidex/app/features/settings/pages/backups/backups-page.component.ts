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
    AppContext,
    BackupDto,
    BackupsService,
    Duration,
    ImmutableArray
} from 'shared';

@Component({
    selector: 'sqx-backups-page',
    styleUrls: ['./backups-page.component.scss'],
    templateUrl: './backups-page.component.html',
    providers: [
        AppContext
    ]
})
export class BackupsPageComponent implements OnInit, OnDestroy {
    private loadSubscription: Subscription;

    public backups = ImmutableArray.empty<BackupDto>();

    constructor(
        public readonly ctx: AppContext,
        private readonly apiUrl: ApiUrlConfig,
        private readonly backupsService: BackupsService
    ) {
    }

    public ngOnDestroy() {
        this.loadSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.loadSubscription =
            Observable.timer(0, 2000)
                .switchMap(t => this.backupsService.getBackups(this.ctx.appName))
                .subscribe(dtos => {
                    this.backups = ImmutableArray.of(dtos);
                });
    }

    public startBackup() {
        this.backupsService.postBackup(this.ctx.appName)
            .subscribe(() => {
                this.ctx.notifyInfo('Backup started, it can take several minutes to complete.');
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public deleteBackup(backup: BackupDto) {
        this.backupsService.deleteBackup(this.ctx.appName, backup.id)
            .subscribe(() => {
                this.ctx.notifyInfo('Backup is about to be deleted.');
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public getDownloadUrl(backup: BackupDto) {
        return this.apiUrl.buildUrl(`api/apps/${this.ctx.appName}/backups/${backup.id}`);
    }

    public getDuration(backup: BackupDto) {
        return Duration.create(backup.started, backup.stopped!);
    }

    public trackBy(index: number, item: BackupDto) {
        return item.id;
    }
}

