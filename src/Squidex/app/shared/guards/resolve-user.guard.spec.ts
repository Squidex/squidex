/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { UserManagementService } from 'shared';

import { ResolveUserGuard } from './resolve-user.guard';
import { RouterMockup } from './router-mockup';

describe('ResolveUserGuard', () => {
    const route = {
        params: {},
        parent: {
            params: {
                userId: 'my-user'
            }
        }
    };

    let usersService: IMock<UserManagementService>;

    beforeEach(() => {
        usersService = Mock.ofType(UserManagementService);
    });

    it('should throw if route does not contain parameter', () => {
        const guard = new ResolveUserGuard(usersService.object, <any>new RouterMockup());

        expect(() => guard.resolve(<any>{ params: {} }, <any>{})).toThrow('Route must contain user id.');
    });

    it('should navigate to 404 page if user is not found', (done) => {
        usersService.setup(x => x.getUser('my-user'))
            .returns(() => Observable.of(null!));
        const router = new RouterMockup();

        const guard = new ResolveUserGuard(usersService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should navigate to 404 page if user loading fails', (done) => {
        usersService.setup(x => x.getUser('my-user'))
            .returns(() => Observable.throw(null!));
        const router = new RouterMockup();

        const guard = new ResolveUserGuard(usersService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return user if loading succeeded', (done) => {
        const user: any = {};

        usersService.setup(x => x.getUser('my-user'))
            .returns(() => Observable.of(user));
        const router = new RouterMockup();

        const guard = new ResolveUserGuard(usersService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBe(user);

                done();
            });
    });
});