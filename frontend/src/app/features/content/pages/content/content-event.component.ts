/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FromNowPipe, TooltipDirective, TranslatePipe } from '@app/shared';
import { ContentDto, HistoryEventDto, HistoryMessagePipe, TypedSimpleChanges, UserNameRefPipe, UserPictureRefPipe } from '@app/shared';

@Component({
    selector: 'sqx-content-event',
    styleUrls: ['./content-event.component.scss'],
    templateUrl: './content-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        TooltipDirective,
        NgIf,
        FromNowPipe,
        TranslatePipe,
        HistoryMessagePipe,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
})
export class ContentEventComponent {
    @Output()
    public dataLoad = new EventEmitter();

    @Output()
    public dataCompare = new EventEmitter();

    @Input({ required: true })
    public event!: HistoryEventDto;

    @Input({ required: true })
    public content!: ContentDto;

    public canLoadOrCompare = false;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.event) {
            this.canLoadOrCompare =
                (this.event.eventType === 'ContentUpdatedEvent' ||
                this.event.eventType === 'ContentCreatedEventV2') &&
                !this.event.version.eq(this.content.version);
        }
    }
}
