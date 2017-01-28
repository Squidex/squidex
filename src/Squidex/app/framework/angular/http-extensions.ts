/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs/Observable';
import { catchError } from './http-utils';

declare module 'rxjs/Observable' {
    interface Observable<T> {
        catchError(message: string): Observable<any>;
    }
}

Observable.prototype['catchError'] = catchError;