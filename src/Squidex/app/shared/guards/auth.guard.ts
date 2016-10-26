/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AuthService } from './../services/auth.service';

const LOGIN_URL = '/login/';
    
@Ng2.Injectable()
export class AuthGuard implements Ng2Router.CanActivate {
    constructor(
        private readonly router: Ng2Router.Router,
        private readonly authService: AuthService
    ) {
    }

    public canActivate(route: Ng2Router.ActivatedRouteSnapshot, state: Ng2Router.RouterStateSnapshot): Promise<boolean> | boolean {
        if (state.url !== LOGIN_URL) {
            return this.authService.checkLogin().then(isAuthenticated => {
                if (!isAuthenticated) {
                    this.router.navigate([LOGIN_URL]);

                    return false;
                }
                return true;
            });
        }

        return true;
    }
}