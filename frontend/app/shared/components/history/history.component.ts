/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { allParams, AppsState, HistoryChannelUpdated, HistoryEventDto, HistoryService, MessageBus, switchSafe } from '@app/shared/internal';
import { merge, Observable, timer } from 'rxjs';
import { delay } from 'rxjs/operators';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HistoryComponent {
    private readonly channel = this.calculateChannel();

    public events: Observable<ReadonlyArray<HistoryEventDto>> =
        merge(
            timer(0, 10000),
            this.messageBus.of(HistoryChannelUpdated).pipe(delay(1000)),
        ).pipe(
            switchSafe(() => this.historyService.getHistory(this.appsState.appName, this.channel)));

    constructor(
        private readonly appsState: AppsState,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
    ) {
    }

    private calculateChannel(): string {
        let channel = this.route.snapshot.data.channel;

        if (channel) {
            const params = allParams(this.route);

            for (const key in params) {
                if (params.hasOwnProperty(key)) {
                    const value = params[key];

                    channel = channel.replace(`{${key}}`, value);
                }
            }
        }

        return channel;
    }
}
