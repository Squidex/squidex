/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Injectable} from '@angular/core';
import { Observable } from 'rxjs';

import { AuthService, Profile } from './../services/auth.service';
import { ApiUrlConfig } from 'framework';

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
            return this.authService.userChanges.first().switchMap(user => {
                return this.makeRequest(req, next, user, true);
            });
        } else {
            return next.handle(req);
        }
    }

    private makeRequest(req: HttpRequest<any>, next: HttpHandler, user: Profile, renew = false): Observable<HttpEvent<any>> {
        const token = user ? user.authToken : '';

        const authReq = req.clone({
            headers: req.headers
                .set('Authorization', token)
                .set('Accept-Language', '*')
                .set('Pragma', 'no-cache')
        });

        return next.handle(authReq)
            .catch((error: HttpErrorResponse) => {
                if (error.status === 401 && renew) {
                    return this.authService.loginSilent().switchMap(u => this.makeRequest(req, next, u));
                } else if (error.status === 401 || error.status === 403) {
                    this.authService.logoutRedirect();

                    return Observable.empty<HttpEvent<any>>();
                }
                return Observable.throw(error);
            });
    }
}
