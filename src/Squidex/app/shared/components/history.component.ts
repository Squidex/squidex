/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';

import { AppComponentBase } from './app.component-base';

import {
    allParams,
    AppsStoreService,
    AuthService,
    DialogService,
    HistoryChannelUpdated,
    HistoryEventDto,
    HistoryService,
    MessageBus,
    UsersProviderService
} from './../declarations-base';

const REPLACEMENT_TEMP = '$TEMP$';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html'
})
export class HistoryComponent extends AppComponentBase {
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
        Observable.timer(0, 10000)
            .merge(this.messageBus.of(HistoryChannelUpdated).delay(1000))
            .switchMap(() => this.appNameOnce())
            .switchMap(app => this.historyService.getHistory(app, this.channel).retry(2));

    constructor(appsStore: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly users: UsersProviderService,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute
    ) {
        super(dialogs, appsStore, authService);
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