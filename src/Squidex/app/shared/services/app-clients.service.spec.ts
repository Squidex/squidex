/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AccessTokenDto,
    AnalyticsService,
    ApiUrlConfig,
    AppClientDto,
    AppClientsDto,
    AppClientsService,
    CreateAppClientDto,
    UpdateAppClientDto,
    Version
} from './../';

describe('AppClientsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppClientsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app clients',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        let clients: AppClientsDto | null = null;

        appClientsService.getClients('my-app').subscribe(result => {
            clients = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                id: 'client1',
                name: 'Client 1',
                secret: 'secret1',
                permission: 'Editor'
            },
            {
                id: 'client2',
                name: 'Client 2',
                secret: 'secret2',
                permission: 'Developer'
            }
        ], {
            headers: {
                etag: '2'
            }
        });

        expect(clients).toEqual(
            new AppClientsDto([
                new AppClientDto('client1', 'Client 1', 'secret1', 'Editor'),
                new AppClientDto('client2', 'Client 2', 'secret2', 'Developer')
            ], new Version('2')));
    }));

    it('should make post request to create client',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        const dto = new CreateAppClientDto('client1');

        let client: AppClientDto | null = null;

        appClientsService.postClient('my-app', dto, version).subscribe(result => {
            client = result.payload;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({ id: 'client1', name: 'Client 1', secret: 'secret1', permission: 'Developer' });

        expect(client).toEqual(
            new AppClientDto('client1', 'Client 1', 'secret1', 'Developer'));
    }));

    it('should make put request to rename client',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        const dto = new UpdateAppClientDto('Client 1 New');

        appClientsService.putClient('my-app', 'client1', dto, version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients/client1');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make delete request to remove client',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        appClientsService.deleteClient('my-app', 'client1', version).subscribe();

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients/client1');

        expect(req.request.method).toEqual('DELETE');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({});
    }));

    it('should make form request to create token',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        let accessTokenDto: AccessTokenDto | null = null;

        appClientsService.createToken('my-app', new AppClientDto('myClientId', 'myClient', 'mySecret', 'Editor')).subscribe(result => {
            accessTokenDto = result;
        });

        const body = 'grant_type=client_credentials&scope=squidex-api&client_id=my-app:myClientId&client_secret=mySecret';

        const req = httpMock.expectOne('http://service/p/identity-server/connect/token');

        expect(req.request.method).toEqual('POST');
        expect(req.request.body).toEqual(body);

        req.flush({ access_token: 'token1', token_type: 'type1' });

        expect(accessTokenDto).toEqual(
            new AccessTokenDto('token1', 'type1'));
    }));
});