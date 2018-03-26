/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { DateTime } from 'shared';

@Component({
    selector: 'sqx-content-status',
    styleUrls: ['./content-status.component.scss'],
    templateUrl: './content-status.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentStatusComponent {
    @Input()
    public status: string;

    @Input()
    public scheduledTo?: string;

    @Input()
    public scheduledAt?: DateTime;

    @Input()
    public hasPendingChanges: any;

    @Input()
    public showLabel = false;

    public get displayStatus() {
        return !!this.hasPendingChanges ? 'Pending' : this.status;
    }
}

