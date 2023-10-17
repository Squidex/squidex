/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable, of, switchMap } from 'rxjs';
import { WebsocketProvider } from 'y-websocket';
import * as Y from 'yjs';
import { ApiUrlConfig } from '../internal';
import { AuthService, Profile } from './auth.service';

@Injectable()
export class CollaborationService {
    private readonly provider = new BehaviorSubject<{ doc: Y.Doc; provider: WebsocketProvider } | null>(null);
    private readonly basePath: string;
    private previousName?: string | null;

    public awareness =
        this.provider.pipe(map(x => x?.provider.awareness));

    public users =
        this.awareness.pipe(
            switchMap(awareness => {
                if (!awareness) {
                    return of([]);
                }

                return new Observable<Profile[]>(observer => {
                    const updateUsers = () => {
                        let result: Profile[] = [];

                        awareness!.getStates().forEach(state => {
                            if (Object.keys(state?.user).length > 0) {
                                result.push(state.user as Profile);
                            }
                        });

                        result.sortByString(x => x.displayName);
                        observer.next(result);
                    };

                    updateUsers();
                    awareness.on('change', updateUsers);

                    return () => {
                        awareness.off('change', updateUsers);
                    };
                });
            }));

    public otherUsers =
        this.users.pipe(map(x => x.filter(u => u.id !== this.authService.user?.id)));

    constructor(apiUrl: ApiUrlConfig,
        private readonly authService: AuthService,
    ) {
        this.basePath =
            apiUrl.buildUrl('/api/')
                .replace('http://', 'ws://')
                .replace('https://', 'wss://');
    }

    public connect(name: string | null) {
        if (name === this.previousName) {
            return;
        }

        this.previousName = name;
        this.cleanup();

        if (!name) {
            return;
        }

        const yDocument = new Y.Doc();
        const yProvider = new WebsocketProvider(this.basePath, `${name}?access_token=${this.authService.user?.accessToken}`, yDocument);

        const user = this.authService.user!;

        if (user) {
            const { id, email, displayName } = user;

            yProvider.awareness?.setLocalStateField('user', { id, email, displayName });
        }

        this.provider.next({ provider: yProvider, doc: yDocument });
    }

    private cleanup() {
        this.provider.value?.provider.destroy();
        this.provider.next(null);
    }

    public getArray<T>(name: string) {
        return this.provider.pipe(map(p => new SharedArray<T>(p?.doc.getArray(name))));
    }

    public getMap<T>(name: string) {
        return this.provider.pipe(map(p => new SharedMap<T>(p?.doc.getMap(name))));
    }
}

export class SharedMap<T> {
    private readonly value$: BehaviorSubject<Readonly<Record<string, T>>>;

    public get valueChanges(): Observable<Readonly<Record<string, T>>> {
        return this.value$;
    }

    public get state() {
        return this.value$.value;
    }

    constructor(
        private readonly source: Y.Map<T> | undefined,
    ) {
        this.value$ = new BehaviorSubject(source?.toJSON() || {});

        source?.observeDeep(() => {
            this.value$.next(source.toJSON());
        });
    }

    public set(key: string, value: T) {
        this.source?.set(key, value);
    }

    public remove(key: string) {
        this.source?.delete(key);
    }
}

export class SharedArray<T> {
    private readonly items$: BehaviorSubject<ReadonlyArray<T>>;

    public get itemsChanges(): Observable<ReadonlyArray<T>> {
        return this.items$;
    }

    public get items() {
        return this.items$.value;
    }

    constructor(
        private readonly source: Y.Array<T> | undefined,
    ) {
        this.items$ = new BehaviorSubject<ReadonlyArray<T>>(source?.toJSON() || []);

        source?.observeDeep(() => {
            this.items$.next(source.toJSON() || []);
        });
    }

    public add(value: T) {
        this.source?.insert(this.source.length, [value]);
    }

    public remove(index: number, count = 1) {
        this.source?.delete(index, count);
    }

    public set(index: number, value: T) {
        this.source?.delete(index, 1);
        this.source?.insert(index, [value]);
    }
}