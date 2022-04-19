/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { finalize, Observable } from 'rxjs';
import { LoadingService, MathHelper } from '@app/framework/internal';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
    constructor(
        private readonly loadingService: LoadingService,
    ) {
    }

    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const id = MathHelper.guid();

        const silent = req.headers.has('X-Silent');

        if (silent) {
            return next.handle(req);
        }

        this.loadingService.startLoading(id);

        return next.handle(req).pipe(finalize(() => {
            this.loadingService.completeLoading(id);
        }));
    }
}
