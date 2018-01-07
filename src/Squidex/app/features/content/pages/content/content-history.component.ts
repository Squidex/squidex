/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Observable } from 'rxjs';

import {
    allParams,
    AppContext,
    HistoryChannelUpdated,
    HistoryEventDto,
    HistoryService,
    UsersProviderService
} from 'shared';

import { ContentVersionSelected } from './../messages';

const REPLACEMENT_TEMP = '$TEMP$';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./content-history.component.scss'],
    templateUrl: './content-history.component.html',
    providers: [
        AppContext
    ]
})
export class ContentHistoryComponent {
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

    private userName(userId: string): Observable<string> {
        const parts = userId.split(':');

        if (parts[0] === 'subject') {
            return this.users.getUser(parts[1], 'Me').map(u => u.displayName);
        } else {
            if (parts[1].endsWith('client')) {
                return Observable.of(parts[1]);
            } else {
                return Observable.of(`${parts[1]}-client`);
            }
        }
    }

    public loadVersion(version: number) {
        this.ctx.bus.emit(new ContentVersionSelected(version));
    }

    public format(message: string): Observable<string> {
        let foundUserId: string | null = null;

        message = message.replace(/{([^\s:]*):([^}]*)}/, (match: string, type: string, id: string) => {
            if (type === 'user') {
                foundUserId = id;
                return REPLACEMENT_TEMP;
            } else {
                return id;
            }
        });

        message = message.replace(/{([^}]*)}/g, (match: string, marker: string) => {
            return `<span class="marker-ref">${marker}</span>`;
        });

        if (foundUserId) {
            return this.userName(foundUserId).map(t => message.replace(REPLACEMENT_TEMP, `<span class="user-ref">${t}</span>`));
        }

        return Observable.of(message);
    }
}