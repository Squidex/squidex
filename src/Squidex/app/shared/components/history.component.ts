/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, Subscription } from 'rxjs';

import {
    ImmutableArray,
    MessageBus,
    NotificationService
} from 'framework';

import { AppComponentBase } from './../app-component-base';
import { AppsStoreService } from './../services/apps-store.service';
import { HistoryChannelUpdated } from './../utils/messages';
import { HistoryEventDto, HistoryService } from './../services/history.service';
import { UsersProviderService } from './../services/users-provider.service';

const FALLBACK_NAME = 'my-app';
const REPLACEMENT_REGEXP = new RegExp('{([^\s:]*):([^}]*)}');
const REPLACEMENT_TEMP = '$TEMP$';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html'
})
export class HistoryComponent extends AppComponentBase {
    public get channel(): string {
        return this.route.snapshot.data['channel'];
    }

    public events =
        Observable.timer(0, 10000)
            .merge(this.messageBus.of(HistoryChannelUpdated).delay(1000))
            .switchMap(() => this.appName())
            .switchMap(app => this.historyService.getHistory(app, this.channel).retry(2));

    constructor(appsStore: AppsStoreService, notifications: NotificationService, usersProvider: UsersProviderService,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute
    ) {
        super(appsStore, notifications, usersProvider);
    }

    public actorName(actor: string): Observable<string> {
        const parts = actor.split(':');

        if (parts[0] === 'subject') {
            return this.userName(parts[1]).map(n => n === 'Me' ? 'I' : n);
        }

        return Observable.of(parts[1]);
    }

    public format(message: string): Observable<string> {
        let foundUserId: string;

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