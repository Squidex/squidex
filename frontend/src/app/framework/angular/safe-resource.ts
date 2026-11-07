/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ResourceRef } from '@angular/core';
import { rxResource, RxResourceOptions } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';

// Errors are swallowed and replaced by the default value, so that reading the value never throws.
export function safeResource<T, P = unknown>(options: RxResourceOptions<T, P> & { defaultValue: T }): ResourceRef<T> {
    return rxResource<T, P>({
        ...options,
        stream: request => options.stream(request).pipe(catchError(() => of(options.defaultValue))),
    });
}
