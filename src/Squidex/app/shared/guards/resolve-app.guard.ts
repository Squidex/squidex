/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from 'framework';

import { AppDto, AppsService } from './../services/apps.service';

@Injectable()
export class ResolveAppGuard implements Resolve<AppDto> {
    constructor(
        private readonly appsService: AppsService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<AppDto | null> {
        const params = allParams(route);

        const appName = params['appName'];

        if (!appName) {
            throw 'Route must contain app name.';
        }

        return this.appsService.getApps().first().map(x => x.find(a => a.name === appName))
            .do(dto => {
                if (!dto) {
                    this.router.navigate(['/404']);
                }
            })
            .catch(error => {
                this.router.navigate(['/404']);

                return Observable.of(null);
            });
    }
}