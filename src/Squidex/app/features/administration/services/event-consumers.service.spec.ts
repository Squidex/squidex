/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import { ApiUrlConfig } from '@app/framework';

import { EventConsumerDto, EventConsumersService } from './event-consumers.service';

describe('EventConsumersService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                EventConsumersService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get event consumers',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {

        let eventConsumers: EventConsumerDto[] | null = null;

        eventConsumersService.getEventConsumers().subscribe(result => {
            eventConsumers = result;
        });

        const req = httpMock.expectOne('http://service/p/api/event-consumers');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                name: 'event-consumer1',
                position: '13',
                isStopped: true,
                isResetting: true,
                error: 'an error 1'
            },
            {
                name: 'event-consumer2',
                position: '29',
                isStopped: true,
                isResetting: true,
                error: 'an error 2'
            }
        ]);

        expect(eventConsumers).toEqual([
            new EventConsumerDto('event-consumer1', true, true, 'an error 1', '13'),
            new EventConsumerDto('event-consumer2', true, true, 'an error 2', '29')
        ]);
    }));

    it('should make put request to start event consumer',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {

        eventConsumersService.putStart('event-consumer1').subscribe();

        const req = httpMock.expectOne('http://service/p/api/event-consumers/event-consumer1/start');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make put request to stop event consumer',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {

        eventConsumersService.putStop('event-consumer1').subscribe();

        const req = httpMock.expectOne('http://service/p/api/event-consumers/event-consumer1/stop');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make put request to reset event consumer',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {

        eventConsumersService.putReset('event-consumer1').subscribe();

        const req = httpMock.expectOne('http://service/p/api/event-consumers/event-consumer1/reset');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));
});