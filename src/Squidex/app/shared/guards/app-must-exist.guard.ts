/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';

import { AppsStoreService } from './../services/apps-store.service';

@Injectable()
export class AppMustExistGuard implements CanActivate {
    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        const appName = route.params['appName'];

        const result =
            this.appsStore.selectApp(appName)
                .then(hasApp => {
                    if (!hasApp) {
                        this.router.navigate(['/404']);
                    }

                    return hasApp;
                }, () => {
                    this.router.navigate(['/404']);

                    return false;
                });

        return result;
    }
}