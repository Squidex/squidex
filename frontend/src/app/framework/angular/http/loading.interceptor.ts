/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';
import { LoadingService, MathHelper } from '@app/framework/internal';

export const loadingInterceptor = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
    const id = MathHelper.guid();

    const silent = req.headers.has('X-Silent');

    if (silent) {
        return next(req);
    }

    const loadingService = inject(LoadingService);

    loadingService.startLoading(id);

    return next(req).pipe(finalize(() => {
        loadingService.completeLoading(id);
    }));
};
