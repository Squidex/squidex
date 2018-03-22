/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Observable } from 'rxjs';

import { AppContext, BackupsService } from 'shared';

@Component({
    selector: 'sqx-backups-page',
    styleUrls: ['./backups-page.component.scss'],
    templateUrl: './backups-page.component.html',
    providers: [
        AppContext
    ]
})
export class BackupsPageComponent {
    public backups =
        Observable.timer(0, 5000)
            .switchMap(t => this.backupsService.getBackups(this.ctx.appName));

    constructor(public readonly ctx: AppContext,
        private readonly backupsService: BackupsService
    ) {
    }

    public startBackup() {
        this.backupsService.postBackup(this.ctx.appName)
            .subscribe(() => {
                this.ctx.notifyInfo('Backup started.');
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public deleteBackup(id: string) {
        this.backupsService.deleteBackup(this.ctx.appName, id)
            .subscribe(() => {
                this.ctx.notifyInfo('Backup deleting.');
            }, error => {
                this.ctx.notifyError(error);
            });
    }
}

