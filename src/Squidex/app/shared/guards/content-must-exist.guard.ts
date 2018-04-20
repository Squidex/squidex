/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from '@app/framework';

import { ContentsState } from './../state/contents.state';

@Injectable()
export class ContentMustExistGuard implements CanActivate {
    constructor(
        private readonly contentsState: ContentsState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const contentId = allParams(route)['contentId'];

        const result =
            this.contentsState.select(contentId)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .map(u => u !== null);

        return result;
    }
}