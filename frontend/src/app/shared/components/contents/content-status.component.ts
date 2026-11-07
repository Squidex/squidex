/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { FullDateTimePipe, LocalizerService, TooltipDirective, TranslatePipe } from '@app/framework';
import { ScheduleJobDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-status',
    styleUrls: ['./content-status.component.scss'],
    templateUrl: './content-status.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FullDateTimePipe,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ContentStatusComponent {
    private readonly localizer = inject(LocalizerService);

    /** The name of the status. */
    public readonly status = input.required<string>();

    /** The color of the status. */
    public readonly statusColor = input.required<string>();

    /** The scheduled status change, if any. */
    public readonly scheduled = input<ScheduleJobDto | null>();

    /** Renders only the icon, the icon with the name or the details of the scheduled change. */
    public readonly layout = input<'icon' | 'text' | 'multiline'>('icon');

    /** Truncates the name when it is too long. */
    public readonly truncate = input(false, { transform: booleanAttribute });

    /** Renders a small icon. */
    public readonly small = input(false, { transform: booleanAttribute });

    protected readonly isMultiline = computed(() => {
        return this.layout() === 'multiline';
    });

    protected readonly isText = computed(() => {
        return this.layout() === 'text';
    });

    protected readonly tooltipText = computed(() => {
        const scheduled = this.scheduled();
        if (!scheduled) {
            return this.status();
        }

        const args = { status: scheduled.status, time: scheduled.dueTime.toStringFormat('PPpp') };

        return this.localizer.getOrKey('i18n:contents.scheduledTooltip', args);
    });
}
