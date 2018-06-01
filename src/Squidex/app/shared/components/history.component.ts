/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Observable, timer } from 'rxjs';
import { delay, merge, switchMap } from 'rxjs/operators';

import {
    allParams,
    AppsState,
    HistoryChannelUpdated,
    HistoryEventDto,
    HistoryService,
    MessageBus
} from '@app/shared/internal';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html'
})
export class HistoryComponent {
    private readonly channel = this.calculateChannel();

    public events: Observable<HistoryEventDto[]> =
        timer(0, 10000).pipe(
                merge(this.messageBus.of(HistoryChannelUpdated).pipe(delay(1000))),
            switchMap(app => this.historyService.getHistory(this.appsState.appName, this.channel)));

    constructor(
        private readonly appsState: AppsState,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute
    ) {
    }

    private calculateChannel(): string {
        let channel = this.route.snapshot.data['channel'];

        if (channel) {
            const params = allParams(this.route);

            for (let key in params) {
                if (params.hasOwnProperty(key)) {
                    const value = params[key];

                    channel = channel.replace(`{${key}}`, value);
                }
            }
        }

        return channel;
    }
}