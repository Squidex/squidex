/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    AccessTokenDto,
    ApiUrlConfig,
    AppClientDto,
    AppClientsService,
    CreateAppClientDto,
    UpdateAppClientDto,
    Version
} from './../';

describe('AppClientDto', () => {
    it('should update name property when renaming', () => {
        const client_1 = new AppClientDto('1', 'old-name', 'secret', false);
        const client_2 = client_1.rename('new-name');

        expect(client_2.name).toBe('new-name');
    });

    it('should update isReader property when changing', () => {
        const client_1 = new AppClientDto('1', 'old-name', 'secret', false);
        const client_2 = client_1.change(true);

        expect(client_2.isReader).toBeTruthy();
    });
});

describe('AppClientsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                AppClientsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app clients',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        let clients: AppClientDto[] | null = null;

        appClientsService.getClients('my-app', version).subscribe(result => {
            clients = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush([
            {
                id: 'client1',
                name: 'Client 1',
                secret: 'secret1',
                isReader: true
            },
            {
                id: 'client2',
                name: 'Client 2',
                secret: 'secret2',
                isReader: true
            }
        ]);

        expect(clients).toEqual(
            [
                new AppClientDto('client1', 'Client 1', 'secret1', true),
                new AppClientDto('client2', 'Client 2', 'secret2', true)
            ]);
    }));

    it('should make post request to create client',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        const dto = new CreateAppClientDto('client1');

        let client: AppClientDto | null = null;

        appClientsService.postClient('my-app', dto, version).subscribe(result => {
            client = result;
        });

        const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toEqual(version.value);

        req.flush({ id: 'client1', name: 'Client 1', secret: 'secret1', isReader: true });

        expect(client).toEqual(
            new AppClientDto('client1', 'Client 1', 'secret1', true));
    }));

    it('should make put request to rename client',
        inject([AppClientsService, HttpTestingController], (appClientsService: AppClientsService, httpMock: HttpTestingController) => {

        const dto = new UpdateAppClientDto('Client 1 New');

        appClientsService.updateClient('my-app', 'client1', dto, version).subscribe();

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

        appClientsService.createToken('my-app', new AppClientDto('myClientId', 'myClient', 'mySecret', false)).subscribe(result => {
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