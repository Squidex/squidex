/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from '@app/framework';

import { AppLanguageDto, AppLanguagesService } from './../services/app-languages.service';

@Injectable()
export class ResolveAppLanguagesGuard implements Resolve<AppLanguageDto[] | null> {
    constructor(
        private readonly appLanguagesService: AppLanguagesService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot): Observable<AppLanguageDto[] | null> {
        const appName = allParams(route)['appName'];

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