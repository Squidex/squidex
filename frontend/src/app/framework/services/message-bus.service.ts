/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DestroyRef, inject, Injectable, NgZone } from '@angular/core';
import { filter, map, Observable, Subject } from 'rxjs';

interface Message {
    // The target.
    channel: string;

    // The message payload.
    data: any;
}

type MessageType<T> = { new(...args: ReadonlyArray<any>): T };

@Injectable({
    providedIn: 'root',
})
export class MessageBus {
    private readonly message$ = new Subject<Message>();

    constructor(
        private readonly zone: NgZone,
    ) {
    }

    public emit<T>(data: T) {
        const channel = ((<any>data)['constructor']).name;

        this.zone.run(() => {
            this.message$.next({ channel, data });
        });
    }

    public of<T>(messageType: MessageType<T>): Observable<T> {
        const channel = (<any>messageType).name;

        return this.message$.pipe(filter(m => m.channel === channel), map(m => m.data));
    }

    public on<T>(messageType: MessageType<T>, handler: (message: T) => void, destroyRef = inject(DestroyRef)) {
        // Unsubscribes automatically when the calling component or service is destroyed.
        const subscription = this.of(messageType).subscribe(handler);

        destroyRef.onDestroy(() => subscription.unsubscribe());
    }
}
