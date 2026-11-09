/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Pipe, PipeTransform, Signal } from '@angular/core';
import { formatHistoryMessage, HistoryEventDto, UsersProviderService } from '@app/shared/internal';

@Pipe({
    name: 'sqxHistoryMessage',
    pure: false,
})
export class HistoryMessagePipe implements PipeTransform {
    private readonly users = inject(UsersProviderService);
    private lastMessage?: string;
    private lastValue?: Signal<string | null>;

    public transform(event: HistoryEventDto): string | null {
        if (!event) {
            return this.lastValue?.() ?? null;
        }

        if (this.lastMessage !== event.message || !this.lastValue) {
            this.lastMessage = event.message;
            this.lastValue = formatHistoryMessage(event.message, this.users);
        }

        return this.lastValue();
    }
}
