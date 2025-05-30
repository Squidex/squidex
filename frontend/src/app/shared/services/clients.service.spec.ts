/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AccessTokenDto, ApiUrlConfig, ClientDto, ClientsDto, ClientsService, Resource, Versioned, VersionTag } from '@app/shared/internal';
import { CreateClientDto, ResourceLinkDto, UpdateClientDto } from '../model';

describe('ClientsService', () => {
    const version = new VersionTag('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        ClientsService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get app clients',
        inject([ClientsService, HttpTestingController], (clientsService: ClientsService, httpMock: HttpTestingController) => {
            let clients: Versioned<ClientsDto>;
            clientsService.getClients('my-app').subscribe(result => {
                clients = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(clientsResponse(1, 2), {
                headers: {
                    etag: '2',
                },
            });

            expect(clients!).toEqual({ payload: createClients(1, 2), version: new VersionTag('2') });
        }));

    it('should make post request to create client',
        inject([ClientsService, HttpTestingController], (clientsService: ClientsService, httpMock: HttpTestingController) => {
            const dto = new CreateClientDto({ id: 'client1' });

            let clients: Versioned<ClientsDto>;
            clientsService.postClient('my-app', dto, version).subscribe(result => {
                clients = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(clientsResponse(1, 2), {
                headers: {
                    etag: '2',
                },
            });

            expect(clients!).toEqual({ payload: createClients(1, 2), version: new VersionTag('2') });
        }));

    it('should make put request to rename client',
        inject([ClientsService, HttpTestingController], (clientsService: ClientsService, httpMock: HttpTestingController) => {
            const dto = new UpdateClientDto({ name: 'New Name' });

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/clients/client1' },
                },
            };

            let clients: Versioned<ClientsDto>;
            clientsService.putClient('my-app', resource, dto, version).subscribe(result => {
                clients = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients/client1');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(clientsResponse(1, 2), {
                headers: {
                    etag: '2',
                },
            });

            expect(clients!).toEqual({ payload: createClients(1, 2), version: new VersionTag('2') });
        }));

    it('should make delete request to remove client',
        inject([ClientsService, HttpTestingController], (clientsService: ClientsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/clients/client1' },
                },
            };

            let clients: Versioned<ClientsDto>;
            clientsService.deleteClient('my-app', resource, version).subscribe(result => {
                clients = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/clients/client1');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(clientsResponse(1, 2), {
                headers: {
                    etag: '2',
                },
            });

            expect(clients!).toEqual({ payload: createClients(1, 2), version: new VersionTag('2') });
        }));

    it('should make form request to create token',
        inject([ClientsService, HttpTestingController], (clientsService: ClientsService, httpMock: HttpTestingController) => {
            let accessTokenDto: AccessTokenDto;

            clientsService.createToken('my-app', createClient(13)).subscribe(result => {
                accessTokenDto = result;
            });

            const body = 'grant_type=client_credentials&scope=squidex-api&client_id=my-app:id13&client_secret=secret13';

            const req = httpMock.expectOne('http://service/p/identity-server/connect/token');

            expect(req.request.method).toEqual('POST');
            expect(req.request.body).toEqual(body);

            req.flush({ access_token: 'token1', token_type: 'type1' });

            expect(accessTokenDto!).toEqual(new AccessTokenDto('token1', 'type1'));
        }));

    function clientsResponse(...ids: number[]) {
        return {
            items: ids.map(id => ({
                id: `id${id}`,
                name: `Client ${id}`,
                role: `Role${id}`,
                secret: `secret${id}`,
                apiCallsLimit: id * 512,
                apiTrafficLimit: id * 5120,
                allowAnonymous: true,
                _links: {
                    update: { method: 'PUT', href: `/clients/id${id}` },
                },
            })),
            _links: {
                create: { method: 'POST', href: '/clients' },
            },
        };
    }
});

export function createClients(...ids: ReadonlyArray<number>) {
    return new ClientsDto({
        items: ids.map(createClient),
        _links: {
            create: new ResourceLinkDto({ method: 'POST', href: '/clients' }),
        },
    });
}

export function createClient(id: number) {
    return new ClientDto({
        id: `id${id}`,
        name: `Client ${id}`,
        role: `Role${id}`,
        secret: `secret${id}`,
        apiCallsLimit: id * 512,
        apiTrafficLimit: id * 5120,
        allowAnonymous: true,
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/clients/id${id}` }),
        },
    });
}