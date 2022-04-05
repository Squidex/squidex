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

    it('should navigate to default page if not authenticated', async () => {
        const authGuard = new MustBeAuthenticatedGuard(uiOptions, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['']), Times.once());
    });

    it('should return true if authenticated', async () => {
        const authGuard = new MustBeAuthenticatedGuard(uiOptions, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should login redirect if redirect enabled', async () => {
        const authGuard = new MustBeAuthenticatedGuard(uiOptionsRedirect, authService.object, router.object);

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate());

        expect(result!).toBeFalsy();

        authService.verify(x => x.loginRedirect(), Times.once());
    });
});
