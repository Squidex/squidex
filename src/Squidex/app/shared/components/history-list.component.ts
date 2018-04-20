/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { Observable } from 'rxjs';

import {
    formatHistoryMessage,
    HistoryEventDto,
    UsersProviderService
} from '@app/shared/internal';

@Component({
    selector: 'sqx-history-list',
    styleUrls: ['./history-list.component.scss'],
    templateUrl: './history-list.component.html'
})
export class HistoryListComponent {
    @Input()
    public events: HistoryEventDto;

    constructor(
        private readonly users: UsersProviderService
    ) {
    }

    public format(message: string): Observable<string> {
        return formatHistoryMessage(message, this.users);
    }

    public trackByEvent(index: number, event: HistoryEventDto) {
        return event.eventId;
    }
}