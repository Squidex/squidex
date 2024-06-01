/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ContentDto, FromNowPipe, HistoryEventDto, HistoryMessagePipe, TooltipDirective, TranslatePipe, TypedSimpleChanges, UserNameRefPipe, UserPictureRefPipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-content-event',
    styleUrls: ['./content-event.component.scss'],
    templateUrl: './content-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FromNowPipe,
        HistoryMessagePipe,
        TooltipDirective,
        TranslatePipe,
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
