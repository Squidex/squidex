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
export class MustBeNotAuthenticatedGuard implements Ng2Router.CanActivate {
    constructor(
        private readonly auth: AuthService,
        private readonly router: Ng2Router.Router
    ) {
    }

    public canActivate(route: Ng2Router.ActivatedRouteSnapshot, state: Ng2Router.RouterStateSnapshot): Promise<boolean> {
        return this.auth.checkLogin().then(isAuthenticated => {
            if (isAuthenticated) {
                this.router.navigate(['app']);
            }
            return !isAuthenticated;
        });
    }
}