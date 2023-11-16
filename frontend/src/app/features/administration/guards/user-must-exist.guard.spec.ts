/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { UserDto, UsersState } from '../internal';
import { userMustExistGuard } from './user-must-exist.guard';

describe('UserMustExistGuard', () => {
    let router: IMock<Router>;
    let usersState: IMock<UsersState>;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        usersState = Mock.ofType<UsersState>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: Router,
                    useValue: router.object,
                },
                {
                    provide: UsersState,
                    useValue: usersState.object,
                },
            ],
        });
    });

    bit('should load user and return true if found', async () => {
        usersState.setup(x => x.select('123'))
            .returns(() => of(<UserDto>{}));

        const route: any = {
            params: {
                userId: '123',
            },
        };

        const result = await firstValueFrom(userMustExistGuard(route));

        expect(result).toBeTruthy();

        usersState.verify(x => x.select('123'), Times.once());
    });

    bit('should load user and return false if not found', async () => {
        usersState.setup(x => x.select('123'))
            .returns(() => of(null));

        const route: any = {
            params: {
                userId: '123',
            },
        };

        const result = await firstValueFrom(userMustExistGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    bit('should unset user if user id is undefined', async () => {
        usersState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                userId: undefined,
            },
        };

        const result = await firstValueFrom(userMustExistGuard(route));

        expect(result).toBeTruthy();

        usersState.verify(x => x.select(null), Times.once());
    });

    bit('should unset user if user id is <new>', async () => {
        usersState.setup(x => x.select(null))
            .returns(() => of(null));

        const route: any = {
            params: {
                userId: 'new',
            },
        };

        const result = await firstValueFrom(userMustExistGuard(route));

        expect(result).toBeTruthy();

        usersState.verify(x => x.select(null), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}