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

    public get displayStatus() {
        return !!this.isPending ? 'Pending' : this.status;
    }
}

