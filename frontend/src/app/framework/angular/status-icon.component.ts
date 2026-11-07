/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TooltipDirective } from './modals/tooltip.directive';

@Component({
    selector: 'sqx-status-icon',
    styleUrls: ['./status-icon.component.scss'],
    templateUrl: './status-icon.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TooltipDirective,
    ],
})
export class StatusIconComponent {
    /** The status to visualize. Unknown values are rendered as pending. */
    public readonly status = input.required<'Started' | 'Failed' | 'Success' | 'Completed' | 'Pending' | string | undefined>();

    /** The tooltip text for the icon. */
    public readonly statusText = input<string | undefined | null>();

    /** The size of the icon. */
    public readonly size = input<'lg' | 'sm'>('lg');
}
