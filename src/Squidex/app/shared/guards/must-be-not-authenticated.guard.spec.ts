/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';

import { AuthService } from 'shared';

import { MustBeNotAuthenticatedGuard } from './must-be-not-authenticated.guard';
import { RouterMockup } from './router-mockup';

describe('MustBeNotAuthenticatedGuard', () => {
    let authService: IMock<AuthService>;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
    });

    it('should navigate to app page if authenticated', (done) => {
        authService.setup(x => x.checkLogin())
            .returns(() => Promise.resolve(true));
        const router = new RouterMockup();

        const guard = new MustBeNotAuthenticatedGuard(authService.object, <any>router);

        guard.canActivate(<any>{}, <any>{})
            .then(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['app']);

                done();
            });
    });

    it('should return true if not authenticated', (done) => {
        authService.setup(x => x.checkLogin())
            .returns(() => Promise.resolve(false));
        const router = new RouterMockup();

        const guard = new MustBeNotAuthenticatedGuard(authService.object, <any>router);

        guard.canActivate(<any>{}, <any>{})
            .then(result => {
                expect(result).toBeTruthy();
                expect(router.lastNavigation).toBeUndefined();

                done();
            });
    });
});