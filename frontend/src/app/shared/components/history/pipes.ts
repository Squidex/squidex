/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { Subscription } from 'rxjs';
import { formatHistoryMessage, HistoryEventDto, UsersProviderService } from '@app/shared/internal';

@Pipe({
    name: 'sqxHistoryMessage',
    pure: false,
})
export class HistoryMessagePipe implements OnDestroy, PipeTransform {
    private subscription?: Subscription;
    private lastMessage?: string;
    private lastValue: string | null = null;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly users: UsersProviderService,
    ) {
    }

    public ngOnDestroy() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    public transform(event: HistoryEventDto): string | null {
        if (!event) {
            return this.lastValue;
        }

        if (this.lastMessage !== event.message) {
            this.lastMessage = event.message;

            if (this.subscription) {
                this.subscription.unsubscribe();
            }

            this.subscription = formatHistoryMessage(event.message, this.users).subscribe(value => {
                this.lastValue = value;

                this.changeDetector.markForCheck();
            });
        }

        return this.lastValue;
    }
}
