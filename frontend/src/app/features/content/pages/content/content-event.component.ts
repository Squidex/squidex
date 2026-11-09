/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, computed, input, output } from '@angular/core';
import { ContentDto, FromNowPipe, HistoryEventDto, HistoryMessagePipe, TooltipDirective, TranslatePipe, UserNameRefPipe, UserPictureRefPipe } from '@app/shared';

@Component({
    selector: 'sqx-content-event',
    styleUrls: ['./content-event.component.scss'],
    templateUrl: './content-event.component.html',
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
    /** Emits when the user wants to load the version of this event. */
    public readonly dataLoad = output();

    /** Emits when the user wants to compare the version of this event with the current version. */
    public readonly dataCompare = output();

    /** The history event to render. */
    public readonly event = input.required<HistoryEventDto>();

    /** The current content. */
    public readonly content = input.required<ContentDto>();

    protected readonly canLoadOrCompare = computed(() => {
        const event = this.event();

        const isContentEvent = event.eventType === 'ContentUpdatedEvent' || event.eventType === 'ContentCreatedEventV2';
        if (!isContentEvent) {
            return false;
        }

        return event.version !== this.content().version;
    });
}
