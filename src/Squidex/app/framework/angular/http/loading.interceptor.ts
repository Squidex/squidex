/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable} from '@angular/core';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

import { LoadingService, MathHelper } from './../../internal';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
    constructor(
        private readonly loadingService: LoadingService
    ) {
    }

    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const id = MathHelper.guid();

        this.loadingService.startLoading(id);

        return next.handle(req).pipe(finalize(() => {
            this.loadingService.completeLoading(id);
        }));
    }
}