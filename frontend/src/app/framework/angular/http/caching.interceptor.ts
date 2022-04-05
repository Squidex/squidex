/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, of, tap, throwError } from 'rxjs';
import { Types } from '@app/framework/internal';

@Injectable()
export class CachingInterceptor implements HttpInterceptor {
    private readonly cache: { [url: string]: HttpResponse<any> } = {};

    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (req.method === 'GET' && !req.reportProgress) {
            const cacheEntry = this.cache[req.url];

            if (cacheEntry) {
                req = req.clone({ headers: req.headers.set('If-None-Match', cacheEntry.headers.get('ETag')!) });
            }

            return next.handle(req).pipe(
                tap(response => {
                    if (Types.is(response, HttpResponse)) {
                        if (response.headers.get('ETag')) {
                            this.cache[req.url] = response;
                        }
                    }
                }),
                catchError(error => {
                    if (Types.is(error, HttpErrorResponse) && error.status === 304 && cacheEntry) {
                        return of(cacheEntry);
                    } else {
                        return throwError(() => error);
                    }
                }));
        } else {
            return next.handle(req);
        }
    }
}
