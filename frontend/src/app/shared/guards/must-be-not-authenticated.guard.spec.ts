/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { AuthService, UIOptions } from '@app/shared/internal';
import { MustBeNotAuthenticatedGuard } from './must-be-not-authenticated.guard';

describe('MustBeNotAuthenticatedGuard', () => {
    let router: IMock<Router>;
    let authService: IMock<AuthService>;
    const uiOptions = new UIOptions({ map: { type: 'OSM' } });
    const uiOptionsRedirect = new UIOptions({ map: { type: 'OSM' }, redirectToLogin: true });

    beforeEach(() => {
        router = Mock.ofType<Router>();

        authService = Mock.ofType<AuthService>();
    });

    it('should navigate to app page if authenticated', async () => {
        const authGuard = new MustBeNotAuthenticatedGuard(uiOptions, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['app']), Times.once());
    });

    it('should return true if not authenticated', async () => {
        const authGuard = new MustBeNotAuthenticatedGuard(uiOptions, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should login redirect and return false if redirect enabled', async () => {
        const authGuard = new MustBeNotAuthenticatedGuard(uiOptionsRedirect, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result).toBeFalsy();

        authService.verify(x => x.loginRedirect(), Times.once());
    });
});
