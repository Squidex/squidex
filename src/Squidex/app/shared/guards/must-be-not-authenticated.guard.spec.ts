/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { AuthService } from '@app/shared';

import { MustBeNotAuthenticatedGuard } from './must-be-not-authenticated.guard';

describe('MustNotBeAuthenticatedGuard', () => {
    let router: IMock<Router>;

    let authService: IMock<AuthService>;
    let authGuard: MustBeNotAuthenticatedGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();

        authService = Mock.ofType<AuthService>();
        authGuard = new MustBeNotAuthenticatedGuard(authService.object, router.object);
    });

    it('should navigate to app page if authenticated', () => {
        authService.setup(x => x.userChanges)
            .returns(() => Observable.of(<any>{}));

        let result: boolean;

        authGuard.canActivate().subscribe(x => {
            result = x;
        });

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['app']), Times.once());
    });

    it('should return true if not authenticated', () => {
        authService.setup(x => x.userChanges)
            .returns(() => Observable.of(null));

        let result: boolean;

        authGuard.canActivate().subscribe(x => {
            result = x;
        });

        expect(result!).toBeTruthy();
    });
});