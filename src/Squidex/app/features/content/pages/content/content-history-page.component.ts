/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { merge, Observable, timer } from 'rxjs';
import { delay, onErrorResumeNext, switchMap } from 'rxjs/operators';

import {
    allParams,
    AppsState,
    HistoryChannelUpdated,
    HistoryEventDto,
    HistoryService,
    MessageBus,
    Version
} from '@app/shared';

import { ContentVersionSelected } from './../messages';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./content-history-page.component.scss'],
    templateUrl: './content-history-page.component.html'
})
export class ContentHistoryPageComponent {
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
        merge(
            timer(0, 10000),
            this.messageBus.of(HistoryChannelUpdated).pipe(delay(1000))
        ).pipe(
            switchMap(() => this.historyService.getHistory(this.appsState.appName, this.channel).pipe(onErrorResumeNext())));

    constructor(
        private readonly appsState: AppsState,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute
    ) {
    }

    public loadVersion(version: number) {
        this.messageBus.emit(new ContentVersionSelected(new Version(version.toString()), false));
    }

    public compareVersion(version: number) {
        this.messageBus.emit(new ContentVersionSelected(new Version(version.toString()), true));
    }

    public trackByEvent(index: number, event: HistoryEventDto) {
        return event.eventId;
    }
}