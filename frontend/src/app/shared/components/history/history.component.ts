/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { merge, Observable, timer } from 'rxjs';
import { delay } from 'rxjs/operators';
import { LayoutComponent } from '@app/framework';
import { allParams, AppsState, HistoryChannelUpdated, HistoryEventDto, HistoryService, MessageBus, SchemasState, switchSafe, TeamsState } from '@app/shared/internal';
import { HistoryListComponent } from './history-list.component';

@Component({
    selector: 'sqx-history',
    styleUrls: ['./history.component.scss'],
    templateUrl: './history.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        HistoryListComponent,
        LayoutComponent,
    ],
})
export class HistoryComponent {
    private readonly appsState = inject(AppsState);
    private readonly historyService = inject(HistoryService);
    private readonly messageBus = inject(MessageBus);
    private readonly route = inject(ActivatedRoute);
    private readonly schemasState = inject(SchemasState);
    private readonly teamsState = inject(TeamsState);

    private readonly channel = this.calculateChannel();

    protected events: Observable<ReadonlyArray<HistoryEventDto>> =
        merge(
            timer(0, 10000),
            this.messageBus.of(HistoryChannelUpdated).pipe(delay(1000)),
        ).pipe(
            switchSafe(() => this.getHistory()));

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
