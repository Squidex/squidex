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
    UpdateWebhookDto,
    Version,
    WebhookDto,
    WebhookEventDto,
    WebhookEventsDto,
    WebhookSchemaDto,
    WebhooksService
} from './../';

describe('WebhookDto', () => {
    const now = DateTime.now();
    const user = 'me';
    const version = new Version('1');

    it('should update url and schemas', () => {
        const webhook_1 = new WebhookDto('id1', 'token1', user, user, now, now, version, [], 'http://squidex.io/hook', 1, 2, 3, 4);
        const webhook_2 =
            webhook_1.update(new UpdateWebhookDto('http://squidex.io/hook2',
            [
                new WebhookSchemaDto('1', true, true, true, true, true),
                new WebhookSchemaDto('2', true, true, true, true, true)
            ]), user, now);

        expect(webhook_2.url).toEqual('http://squidex.io/hook2');
        expect(webhook_2.schemas.length).toEqual(2);
    });
});

describe('WebhooksService', () => {
    const now = DateTime.now();
    const user = 'me';
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
                sharedSecret: 'token1',
                created: '2016-12-12T10:10',
                createdBy: 'CreatedBy1',
                lastModified: '2017-12-12T10:10',
                lastModifiedBy: 'LastModifiedBy1',
                url: 'http://squidex.io/hook',
                version: '1',
                totalSucceeded: 1,
                totalFailed: 2,
                totalTimedout: 3,
                averageRequestTimeMs: 4,
                schemas: [{
                    schemaId: '1',
                    sendCreate: true,
                    sendUpdate: true,
                    sendDelete: true,
                    sendPublish: true,
                    sendUnpublish: true
                }, {
                    schemaId: '2',
                    sendCreate: true,
                    sendUpdate: true,
                    sendDelete: true,
                    sendPublish: true,
                    sendUnpublish: true
                }]
            }
        ]);

        expect(webhooks).toEqual([
            new WebhookDto('id1', 'token1', 'CreatedBy1', 'LastModifiedBy1',
                DateTime.parseISO_UTC('2016-12-12T10:10'),
                DateTime.parseISO_UTC('2017-12-12T10:10'),
                version,
                [
                    new WebhookSchemaDto('1', true, true, true, true, true),
                    new WebhookSchemaDto('2', true, true, true, true, true)
                ],
                'http://squidex.io/hook', 1, 2, 3, 4)
        ]);
    }));

    it('should make post request to create webhook',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        const dto = new CreateWebhookDto('http://squidex.io/hook', []);

        let webhook: WebhookDto | null = null;

        webhooksService.postWebhook('my-app', dto, user, now, version).subscribe(result => {
            webhook = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/webhooks');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({ id: 'id1', sharedSecret: 'token1', schemaId: 'schema1' });

        expect(webhook).toEqual(
            new WebhookDto('id1', 'token1', user, user, now, now, version, [], 'http://squidex.io/hook', 0, 0, 0, 0));
    }));

    it('should make put request to update webhook',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        const dto = new UpdateWebhookDto('http://squidex.io/hook', []);

        webhooksService.putWebhook('my-app', '123', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/webhooks/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to delete webhook',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        webhooksService.deleteWebhook('my-app', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/webhooks/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);
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

    it('should make put request to enqueue webhook event',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        webhooksService.enqueueEvent('my-app', '123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/webhooks/events/123');

        expect(req.request.method).toEqual('PUT');
    }));
});