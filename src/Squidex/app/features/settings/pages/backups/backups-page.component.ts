/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import {
    AppsState,
    BackupDto,
    BackupsState
} from '@app/shared';

@Component({
    selector: 'sqx-backups-page',
    styleUrls: ['./backups-page.component.scss'],
    templateUrl: './backups-page.component.html'
})
export class BackupsPageComponent implements OnInit, OnDestroy {
    private loadSubscription: Subscription;

    constructor(
        public readonly appsState: AppsState,
        public readonly backupsState: BackupsState
    ) {
    }

    public ngOnDestroy() {
        this.loadSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.loadSubscription =
            Observable.timer(0, 2000)
                .switchMap(t => this.backupsState.load().onErrorResumeNext())
                .subscribe();
    }

    public startBackup() {
        this.backupsState.start().onErrorResumeNext().subscribe();
    }

    public deleteBackup(backup: BackupDto) {
        this.backupsState.delete(backup).onErrorResumeNext().subscribe();
    }

    public trackByBackup(index: number, item: BackupDto) {
        return item.id;
    }
}

