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
    UserDto,
    UsersService
} from './../';

describe('UsersService', () => {
    let authService: IMock<AuthService>;
    let usersService: UsersService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        usersService = new UsersService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get many users', () => {
        authService.setup(x => x.authGet('http://service/p/api/users/?query='))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [{
                            id: '123',
                            email: 'mail1@domain.com',
                            displayName: 'User1',
                            pictureUrl: 'path/to/image1'
                        }, {
                            id: '456',
                            email: 'mail2@domain.com',
                            displayName: 'User2',
                            pictureUrl: 'path/to/image2'
                        }]
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UserDto[] = null;

        usersService.getUsers().subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1'),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2')
            ]);

        authService.verifyAll();
    });

    it('should make get request and query to get many users', () => {
        authService.setup(x => x.authGet('http://service/p/api/users/?query=my-query'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [{
                            id: '123',
                            email: 'mail1@domain.com',
                            displayName: 'User1',
                            pictureUrl: 'path/to/image1'
                        }, {
                            id: '456',
                            email: 'mail2@domain.com',
                            displayName: 'User2',
                            pictureUrl: 'path/to/image2'
                        }]
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UserDto[] = null;

        usersService.getUsers('my-query').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1'),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2')
            ]);

        authService.verifyAll();
    });

    it('should make get request to get single user', () => {
        authService.setup(x => x.authGet('http://service/p/api/users/123'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            id: '123',
                            email: 'mail1@domain.com',
                            displayName: 'User1',
                            pictureUrl: 'path/to/image1'
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UserDto = null;

        usersService.getUser('123').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1'));

        authService.verifyAll();
    });
});