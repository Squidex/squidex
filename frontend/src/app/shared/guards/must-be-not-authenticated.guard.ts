/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';
import { UIOptions } from '@app/framework';
import { AuthService } from './../services/auth.service';

@Injectable()
export class MustBeNotAuthenticatedGuard implements CanActivate {

    constructor(
        private readonly authService: AuthService,
        private readonly location: Location,
        private readonly router: Router,
        private readonly uiOptions: UIOptions,
    ) {
    }

    public canActivate(): Observable<boolean> {
        const redirect = this.uiOptions.get('redirectToLogin');

        return this.authService.userChanges.pipe(
            take(1),
            tap(user => {
                const redirectPath = this.location.path(true);

                if (redirect) {
                    this.authService.loginRedirect(redirectPath);
                } else if (user) {
                    this.router.navigate(['app'], { queryParams: { redirectPath } });
                }
            }),
            map(user => !user && !redirect));
    }
}
