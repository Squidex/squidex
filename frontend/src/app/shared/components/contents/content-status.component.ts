/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FullDateTimePipe, LocalizerService, TooltipDirective, TranslatePipe, TypedSimpleChanges } from '@app/framework';
import { ScheduleDto } from '@app/shared/internal';

@Component({
    standalone: true,
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
    @Input({ required: true })
    public status!: string;

    @Input({ required: true })
    public statusColor!: string;

    @Input()
    public scheduled?: ScheduleDto | null;

    @Input()
    public layout: 'icon' | 'text' | 'multiline' = 'icon';

    @Input({ transform: booleanAttribute })
    public truncate?: boolean | null;

    @Input({ transform: booleanAttribute })
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

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.scheduled || changes.status) {
            if (this.scheduled) {
                const args = { status: this.scheduled.status, time: this.scheduled.dueTime.toStringFormat('PPpp') };

                this.tooltipText = this.localizer.getOrKey('i18n:contents.scheduledTooltip', args);
            } else {
                this.tooltipText = this.status;
            }
        }
    }
}
