/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { nextBy } from './rxjs-extensions-impl';

/* tslint:disable:no-shadowed-variable */

declare module 'rxjs/BehaviorSubject' {
    interface BehaviorSubject<T> {
        nextBy(updater: (value: T) => T): void;
    }
}

BehaviorSubject.prototype['nextBy'] = nextBy;