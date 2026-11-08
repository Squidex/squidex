/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */
import { NgZone } from '@angular/core';
import { createService } from '@app/framework/testing';
import { MessageBus } from './message-bus.service';

class Event1 {
}
class Event2 {
}

describe('MessageBus', () => {
    const zone = {
        run: action => {
            action();
        },
    } as NgZone;

    it('should instantiate', () => {
        const messageBus = createService(MessageBus, [
            { provide: NgZone, useValue: zone },
        ]);

        expect(messageBus).toBeDefined();
    });

    it('should publish events and subscribe', () => {
        const messageBus = createService(MessageBus, [
            { provide: NgZone, useValue: zone },
        ]);
        const event1 = new Event1();
        const event2 = new Event2();

        let lastEvent: any = null;

        messageBus.of(Event1).subscribe(event => {
            lastEvent = event;
        });

        messageBus.emit(event1);
        messageBus.emit(event2);

        expect(lastEvent).toBe(event1);
    });
});
