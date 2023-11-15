/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpErrorResponse, HttpHandlerFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { catchError, of, tap, throwError } from 'rxjs';
import { Types } from '@app/framework/internal';

const CACHE: { [url: string]: HttpResponse<any> } = {};

export const cachingInterceptor = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
    if (req.method === 'GET' && !req.reportProgress) {
        const cacheEntry = CACHE[req.url];

        if (cacheEntry) {
            req = req.clone({ headers: req.headers.set('If-None-Match', cacheEntry.headers.get('ETag')!) });
        }

        return next(req).pipe(
            tap(response => {
                if (Types.is(response, HttpResponse)) {
                    if (response.headers.get('ETag')) {
                        CACHE[req.url] = response;
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
        return next(req);
    }
};