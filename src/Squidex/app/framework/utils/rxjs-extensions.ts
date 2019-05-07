/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: only-arrow-functions

import { empty, Observable } from 'rxjs';
import { catchError, map, onErrorResumeNext, publishReplay, refCount, switchMap } from 'rxjs/operators';

import { DialogService } from './../services/dialog.service';

import {
    Version,
    versioned,
    Versioned
} from './version';

export function mapVersioned<T = any, R = any>(project: (value: T, version: Version) => R) {
    return function mapOperation(source: Observable<Versioned<T>>) {
        return source.pipe(map<Versioned<T>, Versioned<R>>(({ version, payload }) => {
            return versioned(version, project(payload, version));
        }));
    };
}

type Options = { silent?: boolean };

export function shareSubscribed<T>(dialogs: DialogService, options?: Options) {
    return shareMapSubscribed<T, T>(dialogs, x => x, options);
}

export function shareMapSubscribed<T, R = T>(dialogs: DialogService, project: (value: T) => R, options?: Options) {
    return function mapOperation(source: Observable<T>) {
        const shared = source.pipe(publishReplay(), refCount());

        shared.pipe(
            catchError(error => {
                if (!options || !options.silent) {
                    dialogs.notifyError(error);
                }

                return empty();
            }))
            .subscribe();

        return shared.pipe(map(x => project(x)));
    };
}

export function switchSafe<T, R>(project: (source: T) => Observable<R>) {
    return function mapOperation(source: Observable<T>) {
        return source.pipe(switchMap(project), onErrorResumeNext<R, R>());
    };
}

export function ofForever<T>(...values: T[]) {
    return new Observable<T>(s => {
        for (let value of values) {
            s.next(value);
        }
    });
}