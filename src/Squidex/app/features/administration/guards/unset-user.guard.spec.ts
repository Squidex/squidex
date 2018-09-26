/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { UsersState } from './../state/users.state';
import { UnsetUserGuard } from './unset-user.guard';

describe('UnsetUserGuard', () => {
    let usersState: IMock<UsersState>;
    let userGuard: UnsetUserGuard;

    beforeEach(() => {
        usersState = Mock.ofType<UsersState>();
        userGuard = new UnsetUserGuard(usersState.object);
    });

    it('should unset user', () => {
        usersState.setup(x => x.select(null))
            .returns(() => of(null));

        let result: boolean;

        userGuard.canActivate().subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        usersState.verify(x => x.select(null), Times.once());
    });
});