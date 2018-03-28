/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs/Observable';

import { pretifyError } from './http-extensions-impl';

/* tslint:disable:no-shadowed-variable */

declare module 'rxjs/Observable' {
    interface Observable<T> {
        pretifyError(message: string): Observable<any>;
    }
}

Observable.prototype['pretifyError'] = pretifyError;