/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { AppDto, HistoryEventDto, HistoryService } from '@app/shared';

@Component({
    selector: 'sqx-history-card',
    styleUrls: ['./history-card.component.scss'],
    templateUrl: './history-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
