/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AppsStoreService } from './../services/apps-store.service';

@Ng2.Injectable()
export class AppMustExistGuard implements Ng2Router.CanActivate {
    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Ng2Router.Router
    ) {
    }

    public canActivate(route: Ng2Router.ActivatedRouteSnapshot, state: Ng2Router.RouterStateSnapshot): Promise<boolean> {
        const appName = route.params['appName'];

        const result =
            this.appsStore.selectApp(appName)
                .then(hasApp => {
                    if (!hasApp) {
                        this.router.navigate(['/404']);
                    }

                    return hasApp;
                }).catch(() => {
                    this.router.navigate(['/404']);
                });

        return result;
    }
}