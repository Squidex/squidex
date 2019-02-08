/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { onErrorResumeNext, switchMap } from 'rxjs/operators';

import {
    AppsState,
    BackupDto,
    BackupsState,
    ResourceOwner
} from '@app/shared';

@Component({
    selector: 'sqx-backups-page',
    styleUrls: ['./backups-page.component.scss'],
    templateUrl: './backups-page.component.html'
})
export class BackupsPageComponent extends ResourceOwner implements OnInit {
    constructor(
        public readonly appsState: AppsState,
        public readonly backupsState: BackupsState
    ) {
        super();
    }

    public ngOnInit() {
        this.backupsState.load().pipe(onErrorResumeNext()).subscribe();

        this.own(
            timer(3000, 3000).pipe(switchMap(() => this.backupsState.load(true, true).pipe(onErrorResumeNext())))
                .subscribe());
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

