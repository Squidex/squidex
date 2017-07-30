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
    Version,
    WebhookDto,
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
                averageRequestTimeMs: 4,
                lastDumps: ['dump1']
            },
            {
                id: 'id2',
                schemaId: 'schemaId2',
                sharedSecret: 'token2',
                url: 'http://squidex.io/2',
                totalSucceeded: 5,
                totalFailed: 6,
                totalTimedout: 7,
                averageRequestTimeMs: 8,
                lastDumps: ['dump2']
            }
        ]);

        expect(webhooks).toEqual([
            new WebhookDto('id1', 'schemaId1', 'token1', 'http://squidex.io/1', 1, 2, 3, 4, ['dump1']),
            new WebhookDto('id2', 'schemaId2', 'token2', 'http://squidex.io/2', 5, 6, 7, 8, ['dump2'])
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

        expect(webhook).toEqual(new WebhookDto('id1', 'schema1', 'token1', dto.url, 0, 0, 0, 0, []));
    }));

    it('should make delete request to delete webhook',
        inject([WebhooksService, HttpTestingController], (webhooksService: WebhooksService, httpMock: HttpTestingController) => {

        webhooksService.deleteWebhook('my-app', 'my-schema', '123', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/schemas/my-schema/webhooks/123');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toBe(version.value);

        req.flush({ id: 'id1', sharedSecret: 'token1' });
    }));
});