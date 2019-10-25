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
    channel: string;
    data: any;
}

export const MessageBusFactory = () => {
    return new MessageBus();
};

@Injectable()
export class MessageBus {
    private message$ = new Subject<Message>();

    public emit<T>(message: T): void {
        const channel = ((<any>message)['constructor']).name;

        this.message$.next({ channel: channel, data: message });
    }

    public of<T>(messageType: { new(...args: ReadonlyArray<any>): T }): Observable<T> {
        const channel = (<any>messageType).name;

        return this.message$.pipe(filter(m => m.channel === channel), map(m => m.data));
    }
}