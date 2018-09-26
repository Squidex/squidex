/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription, timer } from 'rxjs';
import { onErrorResumeNext, switchMap } from 'rxjs/operators';

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
        this.backupsState.load(false, true).pipe(onErrorResumeNext()).subscribe();

        this.timerSubscription =
            timer(3000, 3000).pipe(
                    switchMap(t => this.backupsState.load(true, true)), onErrorResumeNext())
                .subscribe();
    }

    public reload() {
        this.backupsState.load(true, false).pipe(onErrorResumeNext()).subscribe();
    }

    public start() {
        this.backupsState.start().pipe(onErrorResumeNext()).subscribe();
    }

    public delete(backup: BackupDto) {
        this.backupsState.delete(backup).pipe(onErrorResumeNext()).subscribe();
    }

    public trackByBackup(index: number, item: BackupDto) {
        return item.id;
    }
}

