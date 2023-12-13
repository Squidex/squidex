/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { EMPTY, Observable, of, onErrorResumeNextWith, ReplaySubject, throwError } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, filter, map, share, switchMap, tap } from 'rxjs/operators';
import { DialogService } from '../services/dialog.service';
import { Version, versioned, Versioned } from './version';

export function mapVersioned<T = any, R = any>(project: (value: T, version: Version) => R) {
    return function mapOperation(source: Observable<Versioned<T>>) {
        return source.pipe(map<Versioned<T>, Versioned<R>>(({ version, payload }) => {
            return versioned(version, project(payload, version));
        }));
    };
}

type Options = { silent?: boolean; throw?: boolean };

export function shareSubscribed<T>(dialogs?: DialogService, options?: Options) {
    return shareMapSubscribed<T, T>(dialogs, x => x, options);
}

export function shareMapSubscribed<T, R = T>(dialogs: DialogService | undefined, project: (value: T) => R, options?: Options) {
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
                    dialogs?.notifyError(error);
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
            return source.pipe(debounceTime(duration), onErrorResumeNextWith());
        } else {
            return source.pipe(onErrorResumeNextWith());
        }
    };
}

export function defined<T>() {
    return function mapOperation(source: Observable<T | undefined | null>): Observable<T> {
        return source.pipe(filter(x => !!x), map(x => x!), distinctUntilChanged());
    };
}

export function switchSafe<T, R>(project: (source: T) => Observable<R>) {
    return switchMap<T, Observable<R>>(x => {
        try {
            return project(x).pipe(catchError(_ => EMPTY));
        } catch {
            return EMPTY;
        }
    });
}

export function switchMapCached<R>(project: (source: string) => Observable<R>) {
    const cache: { [key: string]: R } = {};

    return switchMap<string, Observable<R>>(x => {
        const cached = cache[x];

        if (cached) {
            return of(cached);
        }

        return project(x).pipe(tap(result => {
            cache[x] = result;
        }));
    });
}