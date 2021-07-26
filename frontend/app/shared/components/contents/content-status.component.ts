/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ScheduleDto } from '@app/shared';

@Component({
    selector: 'sqx-content-status[status][statusColor]',
    styleUrls: ['./content-status.component.scss'],
    templateUrl: './content-status.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentStatusComponent {
    @Input()
    public status: string;

    @Input()
    public statusColor: string;

    @Input()
    public scheduled?: ScheduleDto | null;

    @Input()
    public layout: 'icon' | 'text' | 'multiline' = 'icon';

    @Input()
    public truncate?: boolean | null;

    @Input()
    public small?: boolean | null;

    public get isMultiline() {
        return this.layout === 'multiline';
    }

    public get isText() {
        return this.layout === 'text';
    }

    public get tooltipText() {
        if (this.scheduled) {
            return `Will be set to '${this.scheduled.status}' at ${this.scheduled.dueTime.toStringFormat('PPpp')}`;
        } else {
            return this.status;
        }
    }
}
