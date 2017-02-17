/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AuthService,
    EventConsumerDto,
    EventConsumersService
} from './../';

describe('EventConsumersService', () => {
    let authService: IMock<AuthService>;
    let eventConsumersService: EventConsumersService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        eventConsumersService = new EventConsumersService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get event consumers', () => {
        authService.setup(x => x.authGet('http://service/p/api/event-consumers'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [{
                            name: 'event-consumer1',
                            lastHandledEventNumber: 13,
                            isStopped: true,
                            isResetting: true,
                            error: 'an error 1'
                        }, {
                            name: 'event-consumer2',
                            lastHandledEventNumber: 29,
                            isStopped: true,
                            isResetting: true,
                            error: 'an error 2'
                        }]
                    })
                )
            ))
            .verifiable(Times.once());

        let eventConsumers: EventConsumerDto[] | null = null;

        eventConsumersService.getEventConsumers().subscribe(result => {
            eventConsumers = result;
        }).unsubscribe();

        expect(eventConsumers).toEqual([
            new EventConsumerDto('event-consumer1', 13, true, true, 'an error 1'),
            new EventConsumerDto('event-consumer2', 29, true, true, 'an error 2')
        ]);

        authService.verifyAll();
    });

    it('should make put request to start event consumer', () => {
        authService.setup(x => x.authPut('http://service/p/api/event-consumers/event-consumer1/start', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        eventConsumersService.startEventConsumer('event-consumer1');

        authService.verifyAll();
    });

    it('should make put request to stop event consumer', () => {
        authService.setup(x => x.authPut('http://service/p/api/event-consumers/event-consumer1/stop', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        eventConsumersService.stopEventConsumer('event-consumer1');

        authService.verifyAll();
    });

    it('should make put request to reset event consumer', () => {
        authService.setup(x => x.authPut('http://service/p/api/event-consumers/event-consumer1/reset', It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        eventConsumersService.resetEventConsumer('event-consumer1');

        authService.verifyAll();
    });
});