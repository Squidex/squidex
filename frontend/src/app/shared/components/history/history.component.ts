/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { merge, Observable, timer } from 'rxjs';
import { delay } from 'rxjs/operators';
import { allParams, AppsState, HistoryChannelUpdated, HistoryEventDto, HistoryService, MessageBus, SchemasState, switchSafe, TeamsState } from '@app/shared/internal';

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
            switchSafe(() => this.getHistory()));

    constructor(
        private readonly appsState: AppsState,
        private readonly historyService: HistoryService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly schemasState: SchemasState,
        private readonly teamsState: TeamsState,
    ) {
    }

    private getHistory() {
        if (this.teamsState.teamId) {
            return this.historyService.getHistoryForTeam(this.teamsState.teamId, this.channel);
        } else {
            return this.historyService.getHistory(this.appsState.appName, this.channel);
        }
    }

    private calculateChannel(): string {
        let channel = this.route.snapshot.data.channel;

        if (channel) {
            const params = allParams(this.route);

            channel = channel.replace('{schemaId}', this.schemasState.snapshot.selectedSchema?.id);

            for (const [key, value] of Object.entries(params)) {
                channel = channel.replace(`{${key}}`, value);
            }
        }

        return channel;
    }
}
