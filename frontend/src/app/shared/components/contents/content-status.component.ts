/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { LocalizerService } from '@app/framework';
import { ScheduleDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-status[status][statusColor]',
    styleUrls: ['./content-status.component.scss'],
    templateUrl: './content-status.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentStatusComponent implements OnChanges {
    @Input()
    public status!: string;

    @Input()
    public statusColor!: string;

    @Input()
    public scheduled?: ScheduleDto | null;

    @Input()
    public layout: 'icon' | 'text' | 'multiline' = 'icon';

    @Input()
    public truncate?: boolean | null;

    @Input()
    public small?: boolean | null;

    public tooltipText = '';

    public get isMultiline() {
        return this.layout === 'multiline';
    }

    public get isText() {
        return this.layout === 'text';
    }

    constructor(
        private readonly localizer: LocalizerService,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['scheduled']) {
            if (this.scheduled) {
                const args = { status: this.scheduled.status, time: this.scheduled.dueTime.toStringFormat('PPpp') };

                this.tooltipText = this.localizer.getOrKey('i18n:contents.scheduledTooltip', args);
            } else {
                this.tooltipText = this.status;
            }
        }
    }
}
