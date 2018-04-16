/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
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
    private timerSubscription: Subscription;

    constructor(
        public readonly appsState: AppsState,
        public readonly backupsState: BackupsState
    ) {
    }

    public ngOnDestroy() {
        this.timerSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.backupsState.load(false, true).onErrorResumeNext().subscribe();

        this.timerSubscription =
            Observable.timer(3000, 3000)
                .switchMap(t => this.backupsState.load().onErrorResumeNext())
                .subscribe();
    }

    public reload() {
        this.backupsState.load(true, true).onErrorResumeNext().subscribe();
    }

    public start() {
        this.backupsState.start().onErrorResumeNext().subscribe();
    }

    public delete(backup: BackupDto) {
        this.backupsState.delete(backup).onErrorResumeNext().subscribe();
    }

    public trackByBackup(index: number, item: BackupDto) {
        return item.id;
    }
}

