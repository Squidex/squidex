/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';

import { MessageBus, NotificationService } from 'framework';

import { AppComponentBase }                 from './app-component-base';
import { AppsStoreService }                 from './../services/apps-store.service';
import { HistoryChannelUpdated }            from './../utils/messages';
import { HistoryEventDto, HistoryService }  from './../services/history.service';
import { UsersProviderService }             from './../services/users-provider.service';

const REPLACEMENT_TEMP = '$TEMP$';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html'
})
export class HistoryComponent extends AppComponentBase {
    public get channel(): string {
        let result = this.route.snapshot.data['channel'];
        let params = this.route.parent.snapshot.params;

        for (let key in params) {
            if (params.hasOwnProperty(key)) {
                 const value = params[key];

                 result = result.replace('{' + key + '}', value);
            }
        }

        return result;
    }

    public events: Observable<HistoryEventDto[]> =
        Observable.timer(0, 10000)
            .merge(this.messageBus.of(HistoryChannelUpdated).delay(1000))
            .switchMap(() => this.appName())
            .switchMap(app => this.historyService.getHistory(app, this.channel).retry(2));

    constructor(appsStore: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute
    ) {
        super(notifications, users, appsStore);
    }

    public actorName(actor: string): Observable<string> {
        return this.userName(actor, true, 'I');
    }

    public actorPicture(actor: string): Observable<string> {
        return this.userPicture(actor, true);
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