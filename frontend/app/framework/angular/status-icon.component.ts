/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-status-icon[status]',
    styleUrls: ['./status-icon.component.scss'],
    templateUrl: './status-icon.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusIconComponent {
    @Input()
    public status: 'Started' | 'Failed' | 'Success' | 'Completed' | 'Pending';

    @Input()
    public statusText: string | undefined | null;

    @Input()
    public size: 'lg' | 'sm' = 'lg';
}
