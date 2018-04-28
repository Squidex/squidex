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
    UsersProviderService,
    Version
} from '@app/shared';

import { ContentVersionSelected } from './../messages';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./content-history.component.scss'],
    templateUrl: './content-history.component.html'
})
export class ContentHistoryComponent {
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
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly users: UsersProviderService
    ) {
    }

    public loadVersion(version: number) {
        this.messageBus.emit(new ContentVersionSelected(new Version(version.toString())));
    }

    public format(message: string): Observable<string> {
        return formatHistoryMessage(message, this.users);
    }
}