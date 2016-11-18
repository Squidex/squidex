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
    User,
    UsersProviderService,
    UsersService,
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
        const user = new User('123', 'path/to/image', 'mail@domain.com');

        usersService.setup(x => x.getUser('123'))
            .returns(() => Observable.of(user))
            .verifiable(TypeMoq.Times.once());
            
        let resultingUser: User = null;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser).toBe(user);

        usersService.verifyAll();
    });

    it('Should return provide user from cache', () => {
        const user = new User('123', 'path/to/image', 'mail@domain.com');

        usersService.setup(x => x.getUser('123'))
            .returns(() => Observable.of(user))
            .verifiable(TypeMoq.Times.once());
            
        usersProviderService.getUser('123');
        
        let resultingUser: User = null;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser).toBe(user);

        usersService.verifyAll();
    });

    it('Should return Me when user is current user', () => {
        const user = new User('123', 'path/to/image', 'mail@domain.com');

        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{ profile: { sub: '123'}}));

        usersService.setup(x => x.getUser('123'))
            .returns(() => Observable.of(user))
            .verifiable(TypeMoq.Times.once());
            
        let resultingUser: User = null;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser).toEqual(new User('123', 'path/to/image', 'Me'));

        usersService.verifyAll();
    });
});