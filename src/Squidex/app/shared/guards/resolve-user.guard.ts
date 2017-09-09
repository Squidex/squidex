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

import { UserDto, UserManagementService } from './../services/users.service';

@Injectable()
export class ResolveUserGuard implements Resolve<UserDto | null> {
    constructor(
        private readonly userManagementService: UserManagementService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<UserDto | null> {
        const params = allParams(route);

        const userId = params['userId'];

        if (!userId) {
            throw 'Route must contain user id.';
        }

        const result =
            this.userManagementService.getUser(userId)
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