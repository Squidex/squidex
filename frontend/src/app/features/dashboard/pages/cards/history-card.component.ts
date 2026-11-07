/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input, inject } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { switchMap } from 'rxjs/operators';
import { AppDto, HistoryListComponent, HistoryService, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-history-card',
    styleUrls: ['./history-card.component.scss'],
    templateUrl: './history-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        HistoryListComponent,
        TranslatePipe,
    ],
})
export class HistoryCardComponent {
    /** The app to show the latest history events for. */
    public readonly app = input.required<AppDto>();

    protected readonly history;

    constructor() {
        const historyService = inject(HistoryService);

        this.history = toSignal(
            toObservable(this.app).pipe(
                switchMap(app => historyService.getHistory(app.name, ''))));
    }
}
