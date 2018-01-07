/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { AuthService } from 'shared';

import { MustBeAuthenticatedGuard } from './must-be-authenticated.guard';
import { RouterMockup } from './router-mockup';

describe('MustBeAuthenticatedGuard', () => {
    let authService: IMock<AuthService>;

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
    });

    it('should navigate to default page if not authenticated', (done) => {
        authService.setup(x => x.userChanges)
            .returns(() => Observable.of(null));
        const router = new RouterMockup();

        const guard = new MustBeAuthenticatedGuard(authService.object, <any>router);

        guard.canActivate(<any>{}, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['']);

                done();
            });
    });

    it('should return true if authenticated', (done) => {
        authService.setup(x => x.userChanges)
            .returns(() => Observable.of(<any>{}));
        const router = new RouterMockup();

        const guard = new MustBeAuthenticatedGuard(authService.object, <any>router);

        guard.canActivate(<any>{}, <any>{})
            .subscribe(result => {
                expect(result).toBeTruthy();
                expect(router.lastNavigation).toBeUndefined();

                done();
            });
    });
});