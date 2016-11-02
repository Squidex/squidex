/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AuthService } from './../services/auth.service';
    
@Ng2.Injectable()
export class AuthGuard implements Ng2Router.CanActivate {
    constructor(
        private readonly authService: AuthService
    ) {
    }

    public canActivate(route: Ng2Router.ActivatedRouteSnapshot, state: Ng2Router.RouterStateSnapshot): Promise<boolean> | boolean {
        return this.authService.checkLogin().then(isAuthenticated => {
            if (!isAuthenticated) {
                this.authService.login();

                return false;
            }
            return true;
        });
    }
}