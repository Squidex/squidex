/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { It, IMock, Mock, Times } from 'typemoq';

import {
    ApiUrlConfig,
    AuthService,
    UserManagementService,
    UserDto,
    UsersDto,
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
        authService.setup(x => x.authGet('http://service/p/api/users?query='))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [
                            {
                                id: '123',
                                email: 'mail1@domain.com',
                                displayName: 'User1',
                                pictureUrl: 'path/to/image1',
                                isLocked: true
                            },
                            {
                                id: '456',
                                email: 'mail2@domain.com',
                                displayName: 'User2',
                                pictureUrl: 'path/to/image2',
                                isLocked: true
                            }
                        ]
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UserDto[] | null = null;

        usersService.getUsers().subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
            ]);

        authService.verifyAll();
    });

    it('should make get request with query to get many users', () => {
        authService.setup(x => x.authGet('http://service/p/api/users?query=my-query'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: [
                            {
                                id: '123',
                                email: 'mail1@domain.com',
                                displayName: 'User1',
                                pictureUrl: 'path/to/image1',
                                isLocked: true
                            },
                            {
                                id: '456',
                                email: 'mail2@domain.com',
                                displayName: 'User2',
                                pictureUrl: 'path/to/image2',
                                isLocked: true
                            }
                        ]
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UserDto[] | null = null;

        usersService.getUsers('my-query').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
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
                            pictureUrl: 'path/to/image1',
                            isLocked: true
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UserDto | null = null;

        usersService.getUser('123').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true));

        authService.verifyAll();
    });
});

describe('UserManagementService', () => {
    let authService: IMock<AuthService>;
    let userManagementService: UserManagementService;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        userManagementService = new UserManagementService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get many users', () => {
        authService.setup(x => x.authGet('http://service/p/api/user-management?take=20&skip=30&query='))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 100,
                            items: [
                                {
                                    id: '123',
                                    email: 'mail1@domain.com',
                                    displayName: 'User1',
                                    pictureUrl: 'path/to/image1',
                                    isLocked: true
                                },
                                {
                                    id: '456',
                                    email: 'mail2@domain.com',
                                    displayName: 'User2',
                                    pictureUrl: 'path/to/image2',
                                    isLocked: true
                                }
                            ]
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UsersDto | null = null;

        userManagementService.getUsers(20, 30).subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            new UsersDto(100, [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
            ]));

        authService.verifyAll();
    });

    it('should make get request with query to get many users', () => {
        authService.setup(x => x.authGet('http://service/p/api/user-management?take=20&skip=30&query=my-query'))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            total: 100,
                            items: [
                                {
                                    id: '123',
                                    email: 'mail1@domain.com',
                                    displayName: 'User1',
                                    pictureUrl: 'path/to/image1',
                                    isLocked: true
                                },
                                {
                                    id: '456',
                                    email: 'mail2@domain.com',
                                    displayName: 'User2',
                                    pictureUrl: 'path/to/image2',
                                    isLocked: true
                                }
                            ]
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UsersDto | null = null;

        userManagementService.getUsers(20, 30, 'my-query').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(
            new UsersDto(100, [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
            ]));

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
                            pictureUrl: 'path/to/image1',
                            isLocked: true
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let user: UserDto | null = null;

        userManagementService.getUser('123').subscribe(result => {
            user = result;
        }).unsubscribe();

        expect(user).toEqual(new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true));

        authService.verifyAll();
    });

    it('should make put request to lock user', () => {
        authService.setup(x => x.authPut('http://service/p/api/user-management/123/lock', It.isAny()))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        userManagementService.lockUser('123');

        authService.verifyAll();
    });

    it('should make put request to unlock user', () => {
        authService.setup(x => x.authPut('http://service/p/api/user-management/123/unlock', It.isAny()))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        userManagementService.unlockUser('123');

        authService.verifyAll();
    });
});