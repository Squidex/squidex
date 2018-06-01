/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { AuthService } from '@app/shared';

import { MustBeAuthenticatedGuard } from './must-be-authenticated.guard';

describe('MustBeAuthenticatedGuard', () => {
    let router: IMock<Router>;

    let authService: IMock<AuthService>;
    let authGuard: MustBeAuthenticatedGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();

        authService = Mock.ofType<AuthService>();
        authGuard = new MustBeAuthenticatedGuard(authService.object, router.object);
    });

    it('should navigate to default page if not authenticated', () => {
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
        authService.setup(x => x.userChanges)
            .returns(() => of(<any>{}));

        let result: boolean;

        authGuard.canActivate().subscribe(x => {
            result = x;
        });

        expect(result!).toBeTruthy();
    });
});