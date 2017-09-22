/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

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
        const channel = (<any>message.constructor).name;

        this.message$.next({ channel: channel, data: message });
    }

    public of<T>(messageType: { new(...args: any[]): T }): Observable<T> {
        const channel = (<any>messageType).name;

        return this.message$.filter(m => m.channel === channel).map(m => m.data);
    }
}