/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import {
    ApiUrlConfig,
    BackupDto,
    BackupsState,
    Duration
} from '@app/shared';

@Component({
    selector: 'sqx-backup',
    template: `
        <div class="table-items-row">
            <div class="row">
                <div class="col-auto" [ngSwitch]="backup.status">
                    <sqx-status-icon size="lg" [status]="backup.status"></sqx-status-icon>
                </div>
                <div class="col-auto">
                    <div>
                        Started:
                    </div>
                    <div>
                        Duration:
                    </div>
                </div>
                <div class="col-3">
                    <div>
                        {{backup.started | sqxFromNow}}
                    </div>
                    <div *ngIf="backup.stopped">
                        {{duration}}
                    </div>
                </div>
                <div class="col">
                    <div>
                        <span title="Archived events">
                            Events: <strong class="backup-progress">{{backup.handledEvents | sqxKNumber}}</strong>
                        </span>,
                        <span title="Archived assets">
                            Assets: <strong class="backup-progress">{{backup.handledAssets | sqxKNumber}}</strong>
                        </span>
                    </div>
                    <div *ngIf="backup.stopped && !backup.isFailed">
                        Download:

                        <a href="{{apiUrl.buildUrl(backup.downloadUrl)}}" sqxExternalLink="noicon">
                            Ready <i class="icon-external-link"></i>
                        </a>
                    </div>
                </div>
                <div class="col-auto">
                    <button type="button" class="btn btn-text-danger mt-1"
                        [disabled]="!backup.canDelete"
                        (sqxConfirmClick)="delete()"
                        confirmTitle="Delete backup"
                        confirmText="Do you really want to delete the backup?">
                        <i class="icon-bin2"></i>
                    </button>
                </div>
            </div>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class BackupComponent {
    @Input()
    public backup: BackupDto;

    public get duration() {
        return Duration.create(this.backup.started, this.backup.stopped!).toString();
    }

    constructor(
        public readonly apiUrl: ApiUrlConfig, private readonly backupsState: BackupsState
    ) {
    }

    public delete() {
        this.backupsState.delete(this.backup);
    }
}