/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { AuthService, UIOptions } from '@app/shared/internal';
import { MustBeNotAuthenticatedGuard } from './must-be-not-authenticated.guard';

describe('MustBeNotAuthenticatedGuard', () => {
    let router: IMock<Router>;
    let location: IMock<Location>;
    let authService: IMock<AuthService>;
    let authGuard: MustBeNotAuthenticatedGuard;

    const uiOptions = new UIOptions({});

    beforeEach(() => {
        uiOptions.value.redirectToLogin = false;

        location = Mock.ofType<Location>();
        location.setup(x => x.path(true)).returns(() => '/my-path');

        router = Mock.ofType<Router>();

        TestBed.configureTestingModule({ providers: [{ provide: UIOptions, useValue: uiOptions }] });
        TestBed.runInInjectionContext(() => {
            authService = Mock.ofType<AuthService>();
            authGuard = new MustBeNotAuthenticatedGuard(authService.object, location.object, router.object);
        });
    });

    it('should navigate to app page if authenticated', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(authGuard.canActivate({} as any));

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['app'], { queryParams: { redirectPath: '/my-path' } }), Times.once());
    });

    it('should return true if not authenticated', async () => {
        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate({} as any));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should login redirect and return false if redirect enabled', async () => {
        uiOptions.value.redirectToLogin = true;

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate({ queryParams: {} } as any));

        expect(result).toBeFalsy();

        authService.verify(x => x.loginRedirect('/my-path'), Times.once());
    });

    it('should not redirect after logout', async () => {
        uiOptions.value.redirectToLogin = true;

        authService.setup(x => x.userChanges)
            .returns(() => of(null));

        const result = await firstValueFrom(authGuard.canActivate({ queryParams: { logout: true } } as any));

        expect(result).toBeTruthy();

        authService.verify(x => x.loginRedirect('/my-path'), Times.never());
    });
});
