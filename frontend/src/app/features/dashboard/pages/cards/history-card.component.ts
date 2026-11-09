/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { AppDto, HistoryListComponent, HistoryService, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-history-card',
    styleUrls: ['./history-card.component.scss'],
    templateUrl: './history-card.component.html',
    imports: [
        HistoryListComponent,
        TranslatePipe,
    ],
})
export class HistoryCardComponent {
    private readonly historyService = inject(HistoryService);

    /** The app to show the latest history events for. */
    public readonly app = input.required<AppDto>();

    protected readonly history = rxResource({
        params: () => ({ app: this.app() }),
        stream: ({ params }) => this.historyService.getHistory(params.app.name, ''),
    });
}
