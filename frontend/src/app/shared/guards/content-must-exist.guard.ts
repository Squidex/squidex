/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';
import { allParams } from '@app/framework';
import { ContentsState } from '@app/shared/internal';

export const contentMustExistGuard = (route: ActivatedRouteSnapshot) => {
    const contentsState = inject(ContentsState);
    const contentId = allParams(route)['contentId'];
    const router = inject(Router);

    if (!contentId || contentId === 'new') {
        return contentsState.select(null).pipe(map(u => u === null));
    }

    const decoded = decodeURIComponent(contentId);

    const result =
        contentsState.select(decoded).pipe(
            tap(content => {
                if (!content) {
                    router.navigate(['/404']);
                }
            }),
            map(content => !!content));

    return result;
};
