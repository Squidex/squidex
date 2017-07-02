/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';

import { allParameters } from 'framework';

import { AppLanguageDto, AppLanguagesService } from './../services/app-languages.service';

@Injectable()
export class ResolveAppLanguagesGuard implements Resolve<AppLanguageDto[]> {
    constructor(
        private readonly appLanguagesService: AppLanguagesService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<AppLanguageDto[]> {
        const params = allParameters(route);

        const appName = params['appName'];

        if (!appName) {
            throw 'Route must contain app name.';
        }

        const result =
            this.appLanguagesService.getLanguages(appName).toPromise()
                .then(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);

                        return null;
                    }

                    return dto;
                }).catch(() => {
                    this.router.navigate(['/404']);

                    return null;
                });

        return result;
    }
}