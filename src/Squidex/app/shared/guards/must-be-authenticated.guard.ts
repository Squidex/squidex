/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';

import { AuthService } from './../services/auth.service';

@Injectable()
export class MustBeAuthenticatedGuard implements CanActivate {
    constructor(
        private readonly authService: AuthService,
        private readonly router: Router
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.authService.userChanges.pipe(
            take(1),
            tap(user => {
                if (!user) {
                    this.router.navigate(['']);
                }
            }),
            map(user => !!user));
    }
}