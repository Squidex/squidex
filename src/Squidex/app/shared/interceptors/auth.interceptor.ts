/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable} from '@angular/core';
import { empty, Observable, throwError } from 'rxjs';
import { catchError, switchMap, take } from 'rxjs/operators';

import { ApiUrlConfig } from '@app/framework';

import { AuthService, Profile } from './../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    private baseUrl: string;

    constructor(apiUrlConfig: ApiUrlConfig,
        private readonly authService: AuthService
    ) {
        this.baseUrl = apiUrlConfig.buildUrl('');
    }

    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (req.url.indexOf(this.baseUrl) === 0 && !req.headers.has('NoAuth')) {
            return this.authService.userChanges.pipe(
                take(1),
                switchMap(user => {
                    return this.makeRequest(req, next, user, true);
                }));
        } else {
            return next.handle(req);
        }
    }

    private makeRequest(req: HttpRequest<any>, next: HttpHandler, user: Profile | null, renew = false): Observable<HttpEvent<any>> {
        const token = user ? user.authToken : '';

        const authReq = req.clone({
            headers: req.headers
                .set('Authorization', token)
                .set('Accept-Language', '*')
                .set('Pragma', 'no-cache')
        });

        return next.handle(authReq).pipe(
            catchError((error: HttpErrorResponse) => {
                if (error.status === 401 && renew) {
                    return this.authService.loginSilent().pipe(
                        catchError(_ => {
                            this.authService.logoutRedirect();

                            return empty();
                        }),
                        switchMap(u => this.makeRequest(req, next, u)));
                } else if (error.status === 401 || error.status === 403) {
                    this.authService.logoutRedirect();

                    return empty();
                }

                return throwError(error);
            }));
    }
}
