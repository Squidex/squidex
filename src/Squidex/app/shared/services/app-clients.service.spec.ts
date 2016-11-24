/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as TypeMoq from 'typemoq';
import * as Ng2Http from '@angular/http';

import { Observable } from 'rxjs';

import {
    ApiUrlConfig,
    AppClientDto,
    AppClientsService,
    AuthService,
    AppClientCreateDto,
    DateTime
} from './../';

describe('AppClientsService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let appClientsService: AppClientsService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        appClientsService = new AppClientsService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request with auth service to get app clients', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/clients'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: [{
                            clientName: 'client1',
                            clientSecret: 'secret1',
                            expiresUtc: '2016-12-12T10:10'
                        }, {
                            clientName: 'client2',
                            clientSecret: 'secret2',
                            expiresUtc: '2016-11-11T10:10'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let clients: AppClientDto[] = null;
        
        appClientsService.getClients('my-app').subscribe(result => {
            clients = result;
        }).unsubscribe();

        expect(clients).toEqual(
            [
                new AppClientDto('client1', 'secret1', DateTime.parseISO_UTC('2016-12-12T10:10')),
                new AppClientDto('client2', 'secret2', DateTime.parseISO_UTC('2016-11-11T10:10')),
            ]);

        authService.verifyAll();
    });

    it('should make post request to create client', () => {
        const createClient = new AppClientCreateDto('client1');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/clients', TypeMoq.It.is(c => c === createClient)))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: {
                            clientName: 'client1',
                            clientSecret: 'secret1',
                            expiresUtc: '2016-12-12T10:10'
                        }
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());
        
        let client: AppClientDto = null;

        appClientsService.postClient('my-app', createClient).subscribe(result => {
            client = result;
        });

        expect(client).toEqual(
            new AppClientDto('client1', 'secret1', DateTime.parseISO_UTC('2016-12-12T10:10')));

        authService.verifyAll();
    });

    it('should throw fallback error on 500 when creating client failed', () => {
        const createClient = new AppClientCreateDto('client1');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/clients', TypeMoq.It.is(c => c === createClient)))
            .returns(() => Observable.throw(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        status: 500
                    })
                )
            ));

        let error = '';

        appClientsService.postClient('my-app', createClient).subscribe(x => {}, result => {
            error = result;
        }).unsubscribe();

        expect(error).toBe('A new client could not be created.');

        authService.verifyAll();
    });

    it('should throw duplicate error on 400 when creating client failed', () => {
        const createClient = new AppClientCreateDto('client1');

        authService.setup(x => x.authPost('http://service/p/api/apps/my-app/clients', TypeMoq.It.is(c => c === createClient)))
            .returns(() => Observable.throw(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        status: 400
                    })
                )
            ));

        let error = '';

        appClientsService.postClient('my-app', createClient).subscribe(x => {}, result => {
            error = result;
        }).unsubscribe();

        expect(error).toBe('A client with the same name already exists.');

        authService.verifyAll();
    });

    it('should make delete request to remove client', () => {
        authService.setup(x => x.authDelete('http://service/p/api/apps/my-app/clients/client1'))
            .returns(() => Observable.of(
               new Ng2Http.Response(
                    new Ng2Http.ResponseOptions()
                )
            ))
            .verifiable(TypeMoq.Times.once());

        appClientsService.deleteClient('my-app', 'client1');

        authService.verifyAll();
    });
});