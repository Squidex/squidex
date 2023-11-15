/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FromNowPipe, TooltipDirective } from '@app/framework';
import { HistoryEventDto } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { HistoryMessagePipe } from './pipes';

@Component({
    standalone: true,
    selector: 'sqx-history-list',
    styleUrls: ['./history-list.component.scss'],
    templateUrl: './history-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FromNowPipe,
        HistoryMessagePipe,
        NgFor,
        NgIf,
        TooltipDirective,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
})
export class HistoryListComponent {
    @Input({ required: true })
    public events: ReadonlyArray<HistoryEventDto> | undefined | null;

    public trackByEvent(_index: number, event: HistoryEventDto) {
        return event.eventId;
    }
}
