/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, Resource, ResourceLinks } from '@app/framework';
import { EventConsumerDto, EventConsumersDto, EventConsumersService } from './event-consumers.service';

describe('EventConsumersService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                EventConsumersService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get event consumers',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {
            let eventConsumers: EventConsumersDto;

            eventConsumersService.getEventConsumers().subscribe(result => {
                eventConsumers = result;
            });

            const req = httpMock.expectOne('http://service/p/api/event-consumers');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                items: [
                    eventConsumerResponse(12),
                    eventConsumerResponse(13),
                ],
            });

            expect(eventConsumers!).toEqual(
                new EventConsumersDto([
                    createEventConsumer(12),
                    createEventConsumer(13),
                ]));
        }));

    it('should make put request to start event consumer',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    start: { method: 'PUT', href: 'api/event-consumers/event-consumer123/start' },
                },
            };

            let eventConsumer: EventConsumerDto;

            eventConsumersService.putStart(resource).subscribe(response => {
                eventConsumer = response;
            });

            const req = httpMock.expectOne('http://service/p/api/event-consumers/event-consumer123/start');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(eventConsumerResponse(123));

            expect(eventConsumer!).toEqual(createEventConsumer(123));
        }));

    it('should make put request to stop event consumer',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    stop: { method: 'PUT', href: 'api/event-consumers/event-consumer123/stop' },
                },
            };

            let eventConsumer: EventConsumerDto;

            eventConsumersService.putStop(resource).subscribe(response => {
                eventConsumer = response;
            });

            const req = httpMock.expectOne('http://service/p/api/event-consumers/event-consumer123/stop');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(eventConsumerResponse(12));

            expect(eventConsumer!).toEqual(createEventConsumer(12));
        }));

    it('should make put request to reset event consumer',
        inject([EventConsumersService, HttpTestingController], (eventConsumersService: EventConsumersService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    reset: { method: 'PUT', href: 'api/event-consumers/event-consumer123/reset' },
                },
            };

            let eventConsumer: EventConsumerDto;

            eventConsumersService.putReset(resource).subscribe(response => {
                eventConsumer = response;
            });

            const req = httpMock.expectOne('http://service/p/api/event-consumers/event-consumer123/reset');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(eventConsumerResponse(12));

            expect(eventConsumer!).toEqual(createEventConsumer(12));
        }));

    function eventConsumerResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            name: `event-consumer${id}`,
            position: `position${key}`,
            count: id,
            isStopped: true,
            isResetting: true,
            error: `failure${key}`,
            _links: {
                reset: { method: 'PUT', href: `/event-consumers/${id}/reset` },
            },
        };
    }
});

export function createEventConsumer(id: number, suffix = '') {
    const links: ResourceLinks = {
        reset: { method: 'PUT', href: `/event-consumers/${id}/reset` },
    };

    const key = `${id}${suffix}`;

    return new EventConsumerDto(links,
        `event-consumer${id}`,
        id,
        true,
        true,
        `failure${key}`,
        `position${key}`);
}
