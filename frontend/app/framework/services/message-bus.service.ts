/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { filter, map } from 'rxjs/operators';

interface Message {
    // The target.
    channel: string;

    // The message payload.
    data: any;
}

@Injectable()
export class MessageBus {
    private message$ = new Subject<Message>();

    public emit<T>(data: T) {
        const channel = ((<any>data)['constructor']).name;

        this.message$.next({ channel, data });
    }

    public of<T>(messageType: { new(...args: ReadonlyArray<any>): T }): Observable<T> {
        const channel = (<any>messageType).name;

        return this.message$.pipe(filter(m => m.channel === channel), map(m => m.data));
    }
}
