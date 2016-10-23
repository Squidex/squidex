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

    public canActivate(route: Ng2Router.ActivatedRouteSnapshot, state: Ng2Router.RouterStateSnapshot): boolean {
        if (state.url !== LOGIN_URL && !this.authService.isAuthenticated) {
            this.router.navigate([LOGIN_URL]);
            return false;
        }

        return true;
    }
}