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
    AuthService,
    User,
    UsersService,
} from './../';

describe('UsersService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let usersService: UsersService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        usersService = new UsersService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request with auth service to get many users', () => {
        authService.setup(x => x.authGet('http://service/p/api/users/?query='))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: [{
                            id: '123',
                            profileUrl: 'path/to/image1',
                            displayName: 'mail1@domain.com'
                        }, {
                            id: '456',
                            profileUrl: 'path/to/image2',
                            displayName: 'mail2@domain.com'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let user: User[] = null;
        
        usersService.getUsers().subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            [
                new User('123', 'path/to/image1', 'mail1@domain.com'),
                new User('456', 'path/to/image2', 'mail2@domain.com')
            ]);

        authService.verifyAll();
    });

    it('should make get request with auth service and query to get many users', () => {
        authService.setup(x => x.authGet('http://service/p/api/users/?query=my-query'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: [{
                            id: '123',
                            profileUrl: 'path/to/image1',
                            displayName: 'mail1@domain.com'
                        }, {
                            id: '456',
                            profileUrl: 'path/to/image2',
                            displayName: 'mail2@domain.com'
                        }]
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let user: User[] = null;
        
        usersService.getUsers('my-query').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            [
                new User('123', 'path/to/image1', 'mail1@domain.com'),
                new User('456', 'path/to/image2', 'mail2@domain.com')
            ]);

        authService.verifyAll();
    });

    it('should make get request with auth service to get single user', () => {
        authService.setup(x => x.authGet('http://service/p/api/users/123'))
            .returns(() => Observable.of(
                new Ng2Http.Response(
                    new Ng2Http.ResponseOptions({
                        body: {
                            id: '123',
                            profileUrl: 'path/to/image',
                            displayName: 'mail@domain.com'
                        }
                    })
                )
            ))
            .verifiable(TypeMoq.Times.once());

        let user: User = null;
        
        usersService.getUser('123').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(new User('123', 'path/to/image', 'mail@domain.com'));

        authService.verifyAll();
    });
});