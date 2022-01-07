/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { HistoryEventDto } from '@app/shared/internal';

@Component({
    selector: 'sqx-history-list[events]',
    styleUrls: ['./history-list.component.scss'],
    templateUrl: './history-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HistoryListComponent {
    @Input()
    public events: ReadonlyArray<HistoryEventDto> | undefined | null;

    public trackByEvent(_index: number, event: HistoryEventDto) {
        return event.eventId;
    }
}
