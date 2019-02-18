/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { DateTime } from '@app/shared';

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
    public isPending: any;

    @Input()
    public showLabel = false;

    @Input()
    public alignMiddle = true;

    public get displayStatus() {
        if (this.scheduledAt) {
            return `Will be set to '${this.scheduledTo}' at ${this.scheduledAt.toStringFormat('LLLL')}`;
        } else if (this.isPending) {
            return 'Pending';
        } else {
            return this.status;
        }
    }
}

