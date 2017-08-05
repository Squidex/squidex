/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    CreateWebhookDto,
    DateTime,
    Version,
    WebhookDto,
    WebhookEventDto,
    WebhookEventsDto,
    WebhooksService
} from './../';

describe('WebhooksService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                WebhooksService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app webhooks',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        let webhooks: WebhookDto[] | null = null;

        webhooksService.getWebhooks('my-app', version).subscribe(result => {
            webhooks = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/webhooks');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush([
            {
                id: 'id1',
                schemaId: 'schemaId1',
                sharedSecret: 'token1',
                url: 'http://squidex.io/1',
                totalSucceeded: 1,
                totalFailed: 2,
                totalTimedout: 3,
                averageRequestTimeMs: 4
            },
            {
                id: 'id2',
                schemaId: 'schemaId2',
                sharedSecret: 'token2',
                url: 'http://squidex.io/2',
                totalSucceeded: 5,
                totalFailed: 6,
                totalTimedout: 7,
                averageRequestTimeMs: 8
            }
        ]);

        expect(webhooks).toEqual([
            new WebhookDto('id1', 'schemaId1', 'token1', 'http://squidex.io/1', 1, 2, 3, 4),
            new WebhookDto('id2', 'schemaId2', 'token2', 'http://squidex.io/2', 5, 6, 7, 8)
        ]);
    }));

    it('should make post request to create webhook',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        const dto = new CreateWebhookDto('http://squidex.io/hook');

        let webhook: WebhookDto | null = null;

        webhooksService.postWebhook('my-app', 'my-schema', dto, version).subscribe(result => {
            webhook = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/webhooks');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({ id: 'id1', sharedSecret: 'token1', schemaId: 'schema1' });

        expect(webhook).toEqual(new WebhookDto('id1', 'schema1', 'token1', dto.url, 0, 0, 0, 0));
    }));

    it('should make get request to get app webhook events',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        let webhooks: WebhookEventsDto | null = null;

        webhooksService.getEvents('my-app', 10, 20).subscribe(result => {
            webhooks = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/webhooks/events?take=10&skip=20');

        expect(req.request.method).toEqual('GET');

        req.flush({
            total: 20,
            items: [
                {
                    id: 'id1',
                    created: '2017-12-12T10:10',
                    eventName: 'event1',
                    nextAttempt: '2017-12-12T12:10',
                    jobResult: 'Failed',
                    lastDump: 'dump1',
                    numCalls: 1,
                    requestUrl: 'url1',
                    result: 'Failed'
                },
                {
                    id: 'id2',
                    created: '2017-12-13T10:10',
                    eventName: 'event2',
                    nextAttempt: '2017-12-13T12:10',
                    jobResult: 'Failed',
                    lastDump: 'dump2',
                    numCalls: 2,
                    requestUrl: 'url2',
                    result: 'Failed'
                }
            ]
        });

        expect(webhooks).toEqual(
            new WebhookEventsDto(20, [
                new WebhookEventDto('id1',
                    DateTime.parseISO_UTC('2017-12-12T10:10'),
                    DateTime.parseISO_UTC('2017-12-12T12:10'),
                    'event1', 'url1', 'dump1', 'Failed', 'Failed', 1),
                new WebhookEventDto('id2',
                    DateTime.parseISO_UTC('2017-12-13T10:10'),
                    DateTime.parseISO_UTC('2017-12-13T12:10'),
                    'event2', 'url2', 'dump2', 'Failed', 'Failed', 2)
            ]));
    }));

    it('should make delete request to enqueue webhook event',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        webhooksService.enqueueEvent('my-app', '123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/webhooks/events/123');

        expect(req.request.method).toEqual('PUT');
    }));
});