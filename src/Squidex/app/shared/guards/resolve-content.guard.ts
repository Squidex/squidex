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

import { ContentDto, ContentsService } from './../services/contents.service';

@Injectable()
export class ResolveContentGuard implements Resolve<ContentDto | null> {
    constructor(
        private readonly contentsService: ContentsService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<ContentDto | null> {
        const params = allParams(route);

        const appName = params['appName'];

        if (!appName) {
            throw 'Route must contain app name.';
        }

        const schemaName = params['schemaName'];

        if (!schemaName) {
            throw 'Route must contain schema name.';
        }

        const contentId = params['contentId'];

        if (!contentId) {
            throw 'Route must contain content id.';
        }

        const result =
            this.contentsService.getContent(appName, schemaName, contentId)
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