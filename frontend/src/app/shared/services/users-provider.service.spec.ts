/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { AuthService, Profile, UserDto, UsersProviderService, UsersService } from '@app/shared/internal';

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
        const user = new UserDto({ id: '123', displayName: 'User1' } as any);

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        const resultingUser = usersProviderService.user('123')();

        expect(resultingUser).toBe(user);
        usersService.verifyAll();
    });

    it('should return provide user from cache', () => {
        const user = new UserDto({ id: '123', displayName: 'User1' } as any);

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        usersProviderService.user('123');

        const resultingUser = usersProviderService.user('123')();

        expect(resultingUser).toBe(user);
        usersService.verifyAll();
    });

    it('should provide user from cache for other placeholder', () => {
        const user = new UserDto({ id: '123', displayName: 'User1' } as any);

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        usersProviderService.user('123');

        const resultingUser = usersProviderService.user('123', null)();

        expect(resultingUser).toBe(user);
        usersService.verifyAll();
    });

    it('should return undefined if user not loaded yet', () => {
        usersService.setup(x => x.getUser('123'))
            .returns(() => of());

        const resultingUser = usersProviderService.user('123')();

        expect(resultingUser).toBeUndefined();
    });

    it('should return me if user is current user', () => {
        const user = new UserDto({ id: '123', displayName: 'User1' } as any);

        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{ profile: { sub: '123' } }));

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user)).verifiable(Times.once());

        const resultingUser = usersProviderService.user('123')();

        expect(resultingUser).toEqual(new UserDto({ id: '123', displayName: 'Me' } as any));
        usersService.verifyAll();
    });

    it('should not return me if placeholder is null', () => {
        const user = new UserDto({ id: '123', displayName: 'User1' } as any);

        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{ profile: { sub: '123' } }));

        usersService.setup(x => x.getUser('123'))
            .returns(() => of(user));

        const resultingUser = usersProviderService.user('123', null)();

        expect(resultingUser).toBe(user);
    });

    it('should return invalid user if not found', () => {
        authService.setup(x => x.user)
            .returns(() => new Profile(<any>{ profile: { sub: '123' } }));

        usersService.setup(x => x.getUser('123'))
            .returns(() => throwError(() => 'Service Error')).verifiable(Times.once());

        const resultingUser = usersProviderService.user('123')();

        expect(resultingUser).toEqual(new UserDto({ id: 'Unknown', displayName: 'Unknown' } as any));
        usersService.verifyAll();
    });
});
