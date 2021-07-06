/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AuthService, Profile, UserDto, UsersProviderService, UsersService } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, Mock, Times } from 'typemoq';

describe('UsersProviderService', () => {
    let authService: IMock<AuthService>;
    let usersService: IMock<UsersService>;
    let usersProviderService: UsersProviderService;

    beforeEach(() => {
        authService = Mock.ofType<AuthService>();
        usersService = Mock.ofType<UsersService>();
        usersProviderService = new UsersProviderService(usersService.object, authService.object);
    });

    it('should return users service if user not cached', () => {
        const user = new UserDto('123', 'User1');

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        let resultingUser: UserDto;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser!).toBe(user);

        usersService.verifyAll();
    });

    it('should return provide user from cache', () => {
        const user = new UserDto('123', 'User1');

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        usersProviderService.getUser('123');

        let resultingUser: UserDto;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser!).toBe(user);

        usersService.verifyAll();
    });

    it('should return me if user is current user', () => {
        const user = new UserDto('123', 'User1');

        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{ profile: { sub: '123' } }));

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        let resultingUser: UserDto;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser!).toEqual(new UserDto('123', 'Me'));

        usersService.verifyAll();
    });

    it('should return invalid user if not found', () => {
        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{ profile: { sub: '123' } }));

        usersService.setup(x => x.getUser('123'))
            .returns(() => throwError(() => 'Service Error')).verifiable(Times.once());

        let resultingUser: UserDto;

        usersProviderService.getUser('123').pipe(onErrorResumeNext()).subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser!).toEqual(new UserDto('Unknown', 'Unknown'));

        usersService.verifyAll();
    });
});
