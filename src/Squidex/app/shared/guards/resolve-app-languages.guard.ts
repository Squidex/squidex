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

import { AppLanguageDto, AppLanguagesService } from './../services/app-languages.service';

@Injectable()
export class ResolveAppLanguagesGuard implements Resolve<AppLanguageDto[] | null> {
    constructor(
        private readonly appLanguagesService: AppLanguagesService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<AppLanguageDto[] | null> {
        const params = allParams(route);

        const appName = params['appName'];

        if (!appName) {
            throw 'Route must contain app name.';
        }

        const result =
            this.appLanguagesService.getLanguages(appName).map(d => d.languages)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .catch(error => {
                    this.router.navigate(['/404']);

                    return Observable.of(null);
                });

        return result;
    }
}