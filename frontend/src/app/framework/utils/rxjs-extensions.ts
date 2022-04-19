/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { EMPTY, Observable, ReplaySubject, throwError } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, filter, map, share, switchMap } from 'rxjs/operators';
import { onErrorResumeNext } from 'rxjs/operators';
import { DialogService } from './../services/dialog.service';
import { Version, versioned, Versioned } from './version';

export function mapVersioned<T = any, R = any>(project: (value: T, version: Version) => R) {
    return function mapOperation(source: Observable<Versioned<T>>) {
        return source.pipe(map<Versioned<T>, Versioned<R>>(({ version, payload }) => {
            return versioned(version, project(payload, version));
        }));
    };
}

type Options = { silent?: boolean; throw?: boolean };

export function shareSubscribed<T>(dialogs: DialogService, options?: Options) {
    return shareMapSubscribed<T, T>(dialogs, x => x, options);
}

export function shareMapSubscribed<T, R = T>(dialogs: DialogService, project: (value: T) => R, options?: Options) {
    return function mapOperation(source: Observable<T>) {
        const shared = source.pipe(share({
            connector: () => new ReplaySubject(1),
            resetOnError: false,
            resetOnComplete: false,
            resetOnRefCountZero: false,
        }));

        shared.pipe(
            catchError(error => {
                if (!options || !options.silent) {
                    dialogs.notifyError(error);
                }

                if (options?.throw) {
                    return throwError(() => error);
                }

                return EMPTY;
            }))
            .subscribe();

        return shared.pipe(map(project));
    };
}

export function debounceTimeSafe<T>(duration: number) {
    return function mapOperation(source: Observable<T>) {
        if (duration > 0) {
            return source.pipe(debounceTime(duration), onErrorResumeNext());
        } else {
            return source.pipe(onErrorResumeNext());
        }
    };
}

export function defined<T>() {
    return function mapOperation(source: Observable<T | undefined | null>): Observable<T> {
        return source.pipe(filter(x => !!x), map(x => x!), distinctUntilChanged());
    };
}

export function switchSafe<T, R>(project: (source: T) => Observable<R>) {
    return function mapOperation(source: Observable<T>) {
        return source.pipe(
            switchMap(x => {
                try {
                    return project(x).pipe(catchError(_ => EMPTY));
                } catch {
                    return EMPTY;
                }
            }));
    };
}

export function ofForever<T>(...values: ReadonlyArray<T>) {
    return new Observable<T>(s => {
        for (const value of values) {
            s.next(value);
        }
    });
}
