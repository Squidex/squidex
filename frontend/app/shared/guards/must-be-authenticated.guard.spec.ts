/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { AuthService, UIOptions } from '@app/shared';
import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { MustBeAuthenticatedGuard } from './must-be-authenticated.guard';

describe('MustBeAuthenticatedGuard', () => {
    let router: IMock<Router>;
    let authService: IMock<AuthService>;
    const uiOptions = new UIOptions({ map: { type: 'OSM' } });
    const uiOptionsRedirect = new UIOptions({ map: { type: 'OSM' }, redirectToLogin: true });

    beforeEach(() => {
        router = Mock.ofType<Router>();

        authService = Mock.ofType<AuthService>();
    });

    it('should navigate to default page if not authenticated', () => {
        const authGuard = new MustBeAuthenticatedGuard(uiOptions, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        let result: boolean;

        authGuard.canActivate().subscribe(x => {
            result = x;
        });

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['']), Times.once());
    });

    it('should return true if authenticated', () => {
        const authGuard = new MustBeAuthenticatedGuard(uiOptions, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{}));

        let result: boolean;

        authGuard.canActivate().subscribe(x => {
            result = x;
        });

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should login redirect if redirect enabled', () => {
        const authGuard = new MustBeAuthenticatedGuard(uiOptionsRedirect, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        let result: boolean;

        authGuard.canActivate().subscribe(x => {
            result = x;
        });

        expect(result!).toBeFalsy();

        authService.verify(x => x.loginRedirect(), Times.once());
    });
});
