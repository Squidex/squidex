/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { EMPTY, from, Observable, throwError } from 'rxjs';
import { catchError, switchMap, take } from 'rxjs/operators';
import { ApiUrlConfig, ErrorDto } from '@app/framework';
import { AuthService, Profile } from '../services/auth.service';

type InternalFunction = (user: Profile | null, renew: boolean) => Observable<HttpEvent<any>>;

export const authInterceptor = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
    const authService = inject(AuthService);
    const baseUrl = inject(ApiUrlConfig).buildUrl('');
    const location = inject(Location);
    const router = inject(Router);

    const makeRequest:InternalFunction = (user, renew) => {
        const token = user?.authorization || '';

        req = req.clone({
            headers: req.headers.set('Authorization', token).set('Pragma', 'no-cache'),
        });

        return next(req).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error.status === 401 && renew) {
                    return from(authService.loginSilent()).pipe(
                        catchError(() => {
                            authService.logoutRedirect(location.path());

                            return EMPTY;
                        }),
                        switchMap(u => makeRequest(u, false)));
                } else if (error.status === 401 || error.status === 403) {
                    if (req.method === 'GET') {
                        if (error.status === 401) {
                            authService.logoutRedirect(location.path());
                        } else {
                            router.navigate(['/forbidden'], { replaceUrl: true });
                        }

                        return EMPTY;
                    } else {
                        return throwError(() => new ErrorDto(403, 'i18n:common.errorNoPermission'));
                    }
                }

                return throwError(() => error);
            }));
    };

    if (req.url.indexOf(baseUrl) === 0 && !req.headers.has('NoAuth')) {
        return authService.userChanges.pipe(
            take(1),
            switchMap(user => {
                return makeRequest(user, true);
            }));
    } else {
        return next(req);
    }
};
