/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as TypeMoq from 'typemoq';

import { Observable } from 'rxjs';

import {
    AuthService,
    Profile,
    UserDto,
    UsersProviderService,
    UsersService
} from './../';

describe('UsersProviderService', () => {
    let authService: TypeMoq.Mock<AuthService>;
    let usersService: TypeMoq.Mock<UsersService>;
    let usersProviderService: UsersProviderService;

    beforeEach(() => {
        authService = TypeMoq.Mock.ofType(AuthService);
        usersService = TypeMoq.Mock.ofType(UsersService);
        usersProviderService = new UsersProviderService(usersService.object, authService.object);
    });

    it('Should return users service when user not cached', () => {
        const user = new UserDto('123', 'mail@domain.com', 'User1', 'path/to/image');

        usersService.setup(x => x.getUser('123'))
            .returns(() => Observable.of(user))
            .verifiable(TypeMoq.Times.once());

        let resultingUser: UserDto = null;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser).toBe(user);

        usersService.verifyAll();
    });

    it('Should return provide user from cache', () => {
        const user = new UserDto('123', 'mail@domain.com', 'User1', 'path/to/image');

        usersService.setup(x => x.getUser('123'))
            .returns(() => Observable.of(user))
            .verifiable(TypeMoq.Times.once());

        usersProviderService.getUser('123');

        let resultingUser: UserDto = null;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser).toBe(user);

        usersService.verifyAll();
    });

    it('Should return Me when user is current user', () => {
        const user = new UserDto('123', 'mail@domain.com', 'User1', 'path/to/image');

        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{ profile: { sub: '123'}}));

        usersService.setup(x => x.getUser('123'))
            .returns(() => Observable.of(user))
            .verifiable(TypeMoq.Times.once());

        let resultingUser: UserDto = null;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser).toEqual(new UserDto('123', 'mail@domain.com', 'Me', 'path/to/image'));

        usersService.verifyAll();
    });
});