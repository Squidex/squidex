/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { AppsStoreService } from './../services/apps-store.service';

@Injectable()
export class AppMustExistGuard implements CanActivate {
    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        const appName = route.params['appName'];

        const result =
            this.appsStore.selectApp(appName)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .catch(error => {
                    this.router.navigate(['/404']);

                    return Observable.of(false);
                });

        return result;
    }
}