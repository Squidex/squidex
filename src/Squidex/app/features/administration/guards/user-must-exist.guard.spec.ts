/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { UserDto } from './../services/users.service';
import { UsersState } from './../state/users.state';
import { UserMustExistGuard } from './user-must-exist.guard';

describe('UserMustExistGuard', () => {
    const route: any = {
        params: {
            userId: '123'
        }
    };

    let usersState: IMock<UsersState>;
    let router: IMock<Router>;
    let userGuard: UserMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        usersState = Mock.ofType<UsersState>();
        userGuard = new UserMustExistGuard(usersState.object, router.object);
    });

    it('should load user and return true when found', () => {
        usersState.setup(x => x.select('123'))
            .returns(() => of(<UserDto>{}));

        let result: boolean;

        userGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        usersState.verify(x => x.select('123'), Times.once());
    });

    it('should load user and return false when not found', () => {
        usersState.setup(x => x.select('123'))
            .returns(() => of(null));

        let result: boolean;

        userGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});