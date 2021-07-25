/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { ContentDto, HistoryEventDto } from '@app/shared';

@Component({
    selector: 'sqx-content-event[content][event]',
    styleUrls: ['./content-event.component.scss'],
    templateUrl: './content-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentEventComponent implements OnChanges {
    @Output()
    public dataLoad = new EventEmitter();

    @Output()
    public dataCompare = new EventEmitter();

    @Input()
    public event: HistoryEventDto;

    @Input()
    public content: ContentDto;

    public canLoadOrCompare: boolean;

    public ngOnChanges() {
        this.canLoadOrCompare =
            (this.event.eventType === 'ContentUpdatedEvent' ||
             this.event.eventType === 'ContentCreatedEventV2') &&
            !this.event.version.eq(this.content.version);
    }
}
