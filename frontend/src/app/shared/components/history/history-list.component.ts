/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FromNowPipe, TooltipDirective } from '@app/framework';
import { HistoryEventDto } from '@app/shared/internal';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { HistoryMessagePipe } from './pipes';

@Component({
    selector: 'sqx-history-list',
    styleUrls: ['./history-list.component.scss'],
    templateUrl: './history-list.component.html',
    imports: [
        FromNowPipe,
        HistoryMessagePipe,
        TooltipDirective,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
})
export class HistoryListComponent {
    /** The history events to render, newest first. */
    public readonly events = input.required<ReadonlyArray<HistoryEventDto> | undefined | null>();
}
