/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from '@app/framework';

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
        const contentId = params['contentId'];
        const schemaName = params['schemaName'];

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