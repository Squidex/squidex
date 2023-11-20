/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, NgZone } from '@angular/core';
import { filter, map, Observable, Subject } from 'rxjs';

interface Message {
    // The target.
    channel: string;

    // The message payload.
    data: any;
}

@Injectable({
    providedIn: 'root',
})
export class MessageBus {
    private message$ = new Subject<Message>();

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

    public of<T>(messageType: { new(...args: ReadonlyArray<any>): T }): Observable<T> {
        const channel = (<any>messageType).name;

        return this.message$.pipe(filter(m => m.channel === channel), map(m => m.data));
    }
}
