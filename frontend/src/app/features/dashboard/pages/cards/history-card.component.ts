/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { AppDto, HistoryEventDto, HistoryListComponent, HistoryService, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-history-card',
    styleUrls: ['./history-card.component.scss'],
    templateUrl: './history-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        HistoryListComponent,
        TranslatePipe,
    ],
})
export class HistoryCardComponent {
    @Input({ required: true })
    public app!: AppDto;

    public history?: Observable<ReadonlyArray<HistoryEventDto>>;

    constructor(
        private readonly historyService: HistoryService,
    ) {
    }

    public ngOnChanges() {
        this.history = this.historyService.getHistory(this.app.name, '');
    }
}
