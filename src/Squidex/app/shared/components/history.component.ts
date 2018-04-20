/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';

import {
    allParams,
    AppsState,
    formatHistoryMessage,
    HistoryChannelUpdated,
    HistoryEventDto,
    HistoryService,
    MessageBus,
    UsersProviderService
} from '@app/shared/internal';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html'
})
export class HistoryComponent {
    public get channel(): string {
        let channelPath = this.route.snapshot.data['channel'];

        if (channelPath) {
            const params = allParams(this.route);

            for (let key in params) {
                if (params.hasOwnProperty(key)) {
                    const value = params[key];

                    channelPath = channelPath.replace(`{${key}}`, value);
                }
            }
        }

        return channelPath;
    }

    public events: Observable<HistoryEventDto[]> =
        Observable.timer(0, 10000).merge(this.messageBus.of(HistoryChannelUpdated).delay(1000))
            .switchMap(app => this.historyService.getHistory(this.appsState.appName, this.channel));

    constructor(
        private readonly appsState: AppsState,
        private readonly users: UsersProviderService,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute
    ) {
    }

    public format(message: string): Observable<string> {
        return formatHistoryMessage(message, this.users);
    }
}