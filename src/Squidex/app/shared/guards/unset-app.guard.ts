/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';

import { AppsStoreService } from './../services/apps-store.service';

@Injectable()
export class UnsetAppGuard implements CanActivate {
    constructor(
        private readonly appsStore: AppsStoreService
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        this.appsStore.selectApp(null);

        return true;
    }
}