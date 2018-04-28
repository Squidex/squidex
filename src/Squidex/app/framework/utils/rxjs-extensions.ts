/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';

import { DialogService } from './../services/dialog.service';

import { nextBy, notify } from './rxjs-extensions-impl';

/* tslint:disable:no-shadowed-variable */

declare module 'rxjs/BehaviorSubject' {
    interface Observable<T> {
        notify(dialogs: DialogService): Observable<T>;
    }
}

BehaviorSubject.prototype['nextBy'] = nextBy;

declare module 'rxjs/Observable' {
    interface Observable<T> {
        notify(dialogs: DialogService): Observable<T>;
    }
}

Observable.prototype['notify'] = notify;