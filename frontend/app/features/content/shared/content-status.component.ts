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
    public statusColor: string;

    @Input()
    public scheduledTo?: string;

    @Input()
    public scheduledToColor?: string;

    @Input()
    public scheduledAt?: DateTime;

    @Input()
    public layout: 'icon' | 'text' | 'multiline' = 'icon';

    @Input()
    public small = false;

    public get isMultiline() {
        return this.layout === 'multiline';
    }

    public get isText() {
        return this.layout === 'text';
    }

    public get tooltipText() {
        if (this.scheduledAt) {
            return `Will be set to '${this.scheduledTo}' at ${this.scheduledAt.toStringFormat('LLLL')}`;
        } else {
            return this.status;
        }
    }
}