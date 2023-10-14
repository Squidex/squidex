/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';
import { UIOptions } from '@app/framework';
import { AuthService } from './../services/auth.service';

@Injectable()
export class MustBeAuthenticatedGuard  {
    private readonly redirectToLogin = inject(UIOptions).value.redirectToLogin;

    constructor(
        private readonly authService: AuthService,
        private readonly location: Location,
        private readonly router: Router,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.authService.userChanges.pipe(
            take(1),
            tap(user => {
                if (user) {
                    return;
                }

                const redirectPath = this.location.path(true);

                if (this.redirectToLogin) {
                    this.authService.loginRedirect(redirectPath);
                } else {
                    this.router.navigate([''], { queryParams: { redirectPath } });
                }
            }),
            map(user => !!user));
    }
}
