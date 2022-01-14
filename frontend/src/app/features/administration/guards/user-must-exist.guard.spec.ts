/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { UserDto, UsersState } from '@app/features/administration/internal';
import { UserMustExistGuard } from './user-must-exist.guard';

describe('UserMustExistGuard', () => {
    let usersState: IMock<UsersState>;
    let router: IMock<Router>;
    let userGuard: UserMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        usersState = Mock.ofType<UsersState>();
        userGuard = new UserMustExistGuard(usersState.object, router.object);
    });

    it('should load user and return true if found', async () => {
        usersState.setup(x => x.select('123'))
            .returns(() => of(<UserDto>{}));

        const route: any = {
            params: {
                userId: '123',
            },
        };

        const result = await firstValueFrom(userGuard.canActivate(route));

        expect(result).toBeTruthy();

        usersState.verify(x => x.select('123'), Times.once());
    });

    it('should load user and return false if not found', async () => {
        usersState.setup(x => x.select('123'))
            .returns(() => of(null));

        const route: any = {
            params: {
                userId: '123',
            },
        };

        const result = await firstValueFrom(userGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should unset user if user id is undefined', async () => {
        usersState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                userId: undefined,
            },
        };

        const result = await firstValueFrom(userGuard.canActivate(route));

        expect(result).toBeTruthy();

        usersState.verify(x => x.select(null), Times.once());
    });

    it('should unset user if user id is <new>', async () => {
        usersState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                userId: 'new',
            },
        };

        const result = await firstValueFrom(userGuard.canActivate(route));

        expect(result).toBeTruthy();

        usersState.verify(x => x.select(null), Times.once());
    });
});
