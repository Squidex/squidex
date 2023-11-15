/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';
import { UIOptions } from '@app/framework';
import { AuthService } from '../services/auth.service';

@Injectable()
export class MustBeNotAuthenticatedGuard  {
    private readonly options = inject(UIOptions);

    constructor(
        private readonly authService: AuthService,
        private readonly location: Location,
        private readonly router: Router,
    ) {
    }

    public canActivate(snapshot: ActivatedRouteSnapshot): Observable<boolean> {
        const redirect = this.options.value.redirectToLogin && !snapshot.queryParams.logout;

        return this.authService.userChanges.pipe(
            take(1),
            tap(user => {
                const redirectPath = this.location.path(true);

                if (!user && redirect) {
                    this.authService.loginRedirect(redirectPath);
                } else if (user) {
                    this.router.navigate(['app'], { queryParams: { redirectPath } });
                }
            }),
            map(user => !user && !redirect));
    }
}
