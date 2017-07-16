/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs/Observable';

import { pretifyError } from './http-extensions-impl';

declare module 'rxjs/Observable' {
    interface Observable<T> {
        pretifyError(message: string): Observable<any>;
    }
}

Observable.prototype['pretifyError'] = pretifyError;