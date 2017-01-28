/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';

import { AuthService } from 'shared';

import { MustBeAuthenticatedGuard } from './must-be-authenticated.guard';
import { RouterMockup } from './router-mockup';

describe('MustBeAuthenticatedGuard', () => {
    let authService: IMock<AuthService>;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
    });

    it('should navigate to default page if not authenticated', (done) => {
        authService.setup(x => x.checkLogin())
            .returns(() => Promise.resolve(false));
        const router = new RouterMockup();

        const guard = new MustBeAuthenticatedGuard(authService.object, <any>router);

        guard.canActivate(<any>{}, <any>{})
            .then(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['']);

                done();
            });
    });

    it('should return true if authenticated', (done) => {
        authService.setup(x => x.checkLogin())
            .returns(() => Promise.resolve(true));
        const router = new RouterMockup();

        const guard = new MustBeAuthenticatedGuard(authService.object, <any>router);

        guard.canActivate(<any>{}, <any>{})
            .then(result => {
                expect(result).toBeTruthy();
                expect(router.lastNavigation).toBeUndefined();

                done();
            });
    });
});