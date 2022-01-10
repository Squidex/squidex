/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ApiUrlConfig, BackupDto, BackupsState, ResourceOwner } from '@app/shared';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Component({
    selector: 'sqx-backups-page',
    styleUrls: ['./backups-page.component.scss'],
    templateUrl: './backups-page.component.html',
})
export class BackupsPageComponent extends ResourceOwner implements OnInit {
    constructor(
        public readonly apiUrl: ApiUrlConfig,
        public readonly backupsState: BackupsState,
    ) {
        super();
    }

    public ngOnInit() {
        this.backupsState.load();

        this.own(timer(3000, 3000).pipe(switchMap(() => this.backupsState.load(false, true))));
    }

    public reload() {
        this.backupsState.load(true, false);
    }

    public start() {
        this.backupsState.start();
    }

    public trackByBackup(_index: number, item: BackupDto) {
        return item.id;
    }
}
