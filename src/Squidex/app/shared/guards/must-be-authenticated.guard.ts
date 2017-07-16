/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';

import { AuthService } from './../services/auth.service';

@Injectable()
export class MustBeAuthenticatedGuard implements CanActivate {
    constructor(
        private readonly authService: AuthService,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        return this.authService.checkLogin().then(isAuthenticated => {
            if (!isAuthenticated) {
                this.router.navigate(['']);
            }
            return isAuthenticated;
        });
    }
}