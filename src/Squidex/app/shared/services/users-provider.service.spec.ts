/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    AuthService,
    Profile,
    UserDto,
    UsersProviderService,
    UsersService
} from '@app/shared/internal';

describe('UsersProviderService', () => {
    let authService: IMock<AuthService>;
    let usersService: IMock<UsersService>;
    let usersProviderService: UsersProviderService;

    beforeEach(() => {
        authService = Mock.ofType<AuthService>();
        usersService = Mock.ofType<UsersService>();
        usersProviderService = new UsersProviderService(usersService.object, authService.object);
    });

    it('should return users service when user not cached', () => {
        const user = new UserDto('123', 'User1', 'email');

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
        const user = new UserDto('123', 'User1', 'email');

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

    it('should return me when user is current user', () => {
        const user = new UserDto('123', 'User1', 'email');

        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{
                access_token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMiLCJlbWFpbCI6ImVtYWlsIiwiZ2l2ZW5fbmFtZSI6InRlc3QifQ.2MSPW3IvMx9GvGs4_yVbwEpnMzG_VpmUXPk34c3prnQ',
                profile: { sub: '123' }
            }));

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        let resultingUser: UserDto;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser!).toEqual(new UserDto('123', 'Me', 'email'));

        usersService.verifyAll();
    });

    it('should return invalid user when not found', () => {
        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{
                access_token: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMiLCJlbWFpbCI6ImVtYWlsIiwiZ2l2ZW5fbmFtZSI6InRlc3QifQ.2MSPW3IvMx9GvGs4_yVbwEpnMzG_VpmUXPk34c3prnQ',
                profile: { sub: '123' }
            }));

        usersService.setup(x => x.getUser('123'))
            .returns(() => throwError('NOT FOUND')).verifiable(Times.once());

        let resultingUser: UserDto;

        usersProviderService.getUser('123').subscribe(result => {
            resultingUser = result;
        }).unsubscribe();

        expect(resultingUser!).toEqual(new UserDto('Unknown', 'Unknown', 'Unknown'));

        usersService.verifyAll();
    });
});