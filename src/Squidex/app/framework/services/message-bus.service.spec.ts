/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { MessageBus, MessageBusFactory } from './../';

class Event1 { }
class Event2 { }

describe('MessageBus', () => {
    it('should instantiate from factory', () => {
        const messageBus = MessageBusFactory();

        expect(messageBus).toBeDefined();
    });

    it('should instantiate', () => {
        const messageBus = new MessageBus();

        expect(messageBus).toBeDefined();
    });

    it('should publish events and subscribe', () => {
        const messageBus = new MessageBus();
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