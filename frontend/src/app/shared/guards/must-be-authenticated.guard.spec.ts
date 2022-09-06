/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { AuthService, UIOptions } from '@app/shared/internal';
import { MustBeAuthenticatedGuard } from './must-be-authenticated.guard';

describe('MustBeAuthenticatedGuard', () => {
    let router: IMock<Router>;
    let location: IMock<Location>;
    let authService: IMock<AuthService>;
    let authGuard: MustBeAuthenticatedGuard;

    const uiOptions = new UIOptions({});

    beforeEach(() => {
        uiOptions.value.redirectToLogin = false;

        location = Mock.ofType<Location>();

        location.setup(x => x.path(true))
            .returns(() => '/my-path');

        router = Mock.ofType<Router>();
        authService = Mock.ofType<AuthService>();
        authGuard = new MustBeAuthenticatedGuard(authService.object, location.object, router.object, uiOptions);
    });

    it('should navigate to default page if not authenticated', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result).toBeFalsy();

        router.verify(x => x.navigate([''], { queryParams: { redirectPath: '/my-path' } }), Times.once());
    });

    it('should return true if authenticated', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should login redirect if redirect enabled', async () => {
        uiOptions.value.redirectToLogin = true;

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result!).toBeFalsy();

        authService.verify(x => x.loginRedirect('/my-path'), Times.once());
    });
});
