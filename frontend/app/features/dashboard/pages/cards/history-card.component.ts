/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { AppDto, fadeAnimation, HistoryEventDto, HistoryService } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-history-card[app]',
    styleUrls: ['./history-card.component.scss'],
    templateUrl: './history-card.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HistoryCardComponent implements OnChanges {
    @Input()
    public app: AppDto;

    public history: Observable<ReadonlyArray<HistoryEventDto>>;

    constructor(
        private readonly historyService: HistoryService,
    ) {
    }

    public ngOnChanges() {
        this.history = this.historyService.getHistory(this.app.name, '');
    }
}
