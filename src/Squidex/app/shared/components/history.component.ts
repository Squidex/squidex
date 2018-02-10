/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Observable } from 'rxjs';

import { AppContext } from './app-context';

import {
    allParams,
    formatHistoryMessage,
    HistoryChannelUpdated,
    HistoryEventDto,
    HistoryService,
    UsersProviderService
} from './../declarations-base';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html',
    providers: [
        AppContext
    ]
})
export class HistoryComponent {
    public get channel(): string {
        let channelPath = this.ctx.route.snapshot.data['channel'];

        if (channelPath) {
            const params = allParams(this.ctx.route);

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
        Observable.timer(0, 10000).merge(this.ctx.bus.of(HistoryChannelUpdated).delay(1000))
            .switchMap(app => this.historyService.getHistory(this.ctx.appName, this.channel));

    constructor(public readonly ctx: AppContext,
        private readonly users: UsersProviderService,
        private readonly historyService: HistoryService
    ) {
    }

    public format(message: string): Observable<string> {
        return formatHistoryMessage(message, this.users);
    }
}