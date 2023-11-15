/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TooltipDirective } from './modals/tooltip.directive';

@Component({
    selector: 'sqx-status-icon',
    styleUrls: ['./status-icon.component.scss'],
    templateUrl: './status-icon.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgSwitch,
        NgSwitchCase,
        TooltipDirective,
        NgSwitchDefault,
    ],
})
export class StatusIconComponent {
    @Input({ required: true })
    public status?: 'Started' | 'Failed' | 'Success' | 'Completed' | 'Pending' | string;

    @Input()
    public statusText: string | undefined | null;

    @Input()
    public size: 'lg' | 'sm' = 'lg';
}
