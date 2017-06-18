/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AuthService,
    CreateWebhookDto,
    Version,
    WebhookCreatedDto,
    WebhookDto,
    WebhooksService
} from './../';

describe('WebhooksService', () => {
    let authService: IMock<AuthService>;
    let webhooksService: WebhooksService;
    let version = new Version('1');

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        webhooksService = new WebhooksService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get app webhooks', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/webhooks', version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [{
                            id: 'id1',
                            schemaId: 'schemaId1',
                            sharedSecret: 'token1',
                            url: 'http://squidex.io/1',
                            totalSucceeded: 1,
                            totalFailed: 2,
                            totalTimedout: 3,
                            averageRequestTimeMs: 4,
                            dumps: ['dump1']
                        }, {
                            id: 'id2',
                            schemaId: 'schemaId2',
                            sharedSecret: 'token2',
                            url: 'http://squidex.io/2',
                            totalSucceeded: 5,
                            totalFailed: 6,
                            totalTimedout: 7,
                            averageRequestTimeMs: 8,
                            dumps: ['dump2']
                        }]
                    })
                )
            ))
            .verifiable(Times.once());

        let webhooks: WebhookDto[] | null = null;

        webhooksService.getWebhooks('my-app', version).subscribe(result => {
            webhooks = result;
        }).unsubscribe();

        expect(webhooks).toEqual([
            new WebhookDto('id1', 'schemaId1', 'token1', 'http://squidex.io/1', 1, 2, 3, 4, ['dump1']),
            new WebhookDto('id2', 'schemaId2', 'token2', 'http://squidex.io/2', 5, 6, 7, 8, ['dump2'])
        ]);

        authService.verifyAll();
    });

    it('should make post request to create webhook', () => {
        const dto = new CreateWebhookDto('http://squidex.io/hook');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/schemas/my-schema/webhooks', dto, version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: { id: 'id1', sharedSecret: 'token1' }
                    })
                )
            ))
            .verifiable(Times.once());

        let webhook: WebhookCreatedDto | null = null;

        webhooksService.postWebhook('my-app', 'my-schema', dto, version).subscribe(result => {
            webhook = result;
        }).unsubscribe();

        expect(webhook).toEqual(new WebhookCreatedDto('id1', 'token1'));

        authService.verifyAll();
    });

    it('should make delete request to delete webhook', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/schemas/my-schema/webhooks/123', version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        webhooksService.deleteWebhook('my-app', 'my-schema', '123', version);

        authService.verifyAll();
    });
});