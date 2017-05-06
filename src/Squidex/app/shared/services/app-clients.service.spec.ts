/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Http, Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { It, IMock, Mock, Times } from 'typemoq';

import {
    AccessTokenDto,
    ApiUrlConfig,
    AppClientDto,
    AppClientsService,
    AuthService,
    CreateAppClientDto,
    UpdateAppClientDto,
    Version
} from './../';

describe('AppClientsService', () => {
    let authService: IMock<AuthService>;
    let appClientsService: AppClientsService;
    let version = new Version('1');
    let http: IMock<Http>;

    beforeEach(() => {
        http = Mock.ofType(Http);

        authService = Mock.ofType(AuthService);
        appClientsService = new AppClientsService(authService.object, new ApiUrlConfig('http://service/p/'), http.object);
    });

    it('should make get request to get app clients', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/clients', version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [
                            {
                                id: 'client1',
                                name: 'Client 1',
                                secret: 'secret1'
                            },
                            {
                                id: 'client2',
                                name: 'Client 2',
                                secret: 'secret2'
                            }
                        ]
                    })
                )
            ))
            .verifiable(Times.once());

        let clients: AppClientDto[] | null = null;

        appClientsService.getClients('my-app', version).subscribe(result => {
            clients = result;
        }).unsubscribe();

        expect(clients).toEqual(
            [
                new AppClientDto('client1', 'Client 1', 'secret1'),
                new AppClientDto('client2', 'Client 2', 'secret2')
            ]);

        authService.verifyAll();
    });

    it('should make post request to create client', () => {
        const dto = new CreateAppClientDto('client1');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/clients', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            id: 'client1',
                            name: 'Client 1',
                            secret: 'secret1'
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let client: AppClientDto | null = null;

        appClientsService.postClient('my-app', dto, version).subscribe(result => {
            client = result;
        });

        expect(client).toEqual(
            new AppClientDto('client1', 'Client 1', 'secret1'));

        authService.verifyAll();
    });

    it('should make put request to rename client', () => {
        const dto = new UpdateAppClientDto('Client 1 New');

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/clients/client1', dto, version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appClientsService.updateClient('my-app', 'client1', dto, version);

        authService.verifyAll();
    });

    it('should make delete request to remove client', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/clients/client1', version))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        appClientsService.deleteClient('my-app', 'client1', version);

        authService.verifyAll();
    });

    it('should make form request to create token', () => {
        const body = 'grant_type=client_credentials&scope=squidex-api&client_id=my-app:myClientId&client_secret=mySecret';

        http.setup(x => x.post('http://service/p/identity-server/connect/token', body, It.isAny()))
            .returns(() => Observable.of(
               new Response(
                    new ResponseOptions({
                        body: {
                            access_token: 'token1', token_type: 'type1'
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let accessTokenDto: AccessTokenDto | null = null;

        appClientsService.createToken('my-app', new AppClientDto('myClientId', 'myClient', 'mySecret')).subscribe(result => {
            accessTokenDto = result;
        });

        expect(accessTokenDto).toEqual(
            new AccessTokenDto('token1', 'type1'));

        http.verifyAll();
    });
});