/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, distinctUntilChanged, map, Observable, of, shareReplay, switchMap } from 'rxjs';
import { Awareness } from 'y-protocols/awareness';
import { WebsocketProvider } from 'y-websocket';
import * as Y from 'yjs';
import { Types, UIOptions } from '@app/framework';
import { AuthService, Profile } from './auth.service';

type AwarenessState = { user: Profile; [key: string]: any };

export type CollaborationProvider = { awareness: Awareness; doc: Y.Doc; destroy: () => void };

@Injectable()
export class CollaborationService {
    private readonly provider = new BehaviorSubject<CollaborationProvider | null>(null);
    private readonly basePath = inject(UIOptions).value.collaborationService;
    private readonly profile: Profile;
    private previousName?: string | null;

    public awarenessChanges =
        this.provider.pipe(map(p => p?.awareness));

    public userChanges =
        this.awarenessChanges.pipe(
            switchMap(awareness => {
                if (!awareness) {
                    return of([]);
                }

                return new Observable<AwarenessState[]>(observer => {
                    const getStates = () => {
                        const byUser: { [userId: string]: AwarenessState } = {};

                        awareness!.getStates().forEach((state, clientId) => {
                            if (clientId == awareness.clientID) {
                                return;
                            }

                            if (!state.user || state.user.id === this.profile.id) {
                                return;
                            }

                            byUser[state.user.id] = state as any;
                        });

                        const sorted = Object.values(byUser).sortByString(x => x.user.displayName);

                        observer.next(sorted);
                    };

                    getStates();
                    awareness.on('change', getStates);

                    return () => {
                        awareness.off('change', getStates);
                    };
                });
            }),
            distinctUntilChanged<AwarenessState[]>(Types.equals),
            shareReplay());

    public providerFactory: (name: string, doc: Y.Doc) => CollaborationProvider;

    constructor(authService: AuthService) {
        this.profile = authService.user!;

        this.providerFactory = (name, doc) => {
            const websocket = new WebsocketProvider(this.basePath, `api/${name}?access_token=${this.profile.accessToken}`, doc);

            return { doc, awareness: websocket.awareness, destroy: () => websocket.destroy() };
        };
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
        const yProvider = this.providerFactory(name, yDocument);

        const { id, email, displayName } = this.profile;

        yProvider.awareness?.setLocalStateField('user', { id, email, displayName });

        this.provider.next(yProvider);
    }

    private cleanup() {
        this.provider.value?.destroy();
        this.provider.next(null);
    }

    public getArray<T>(name: string) {
        return this.provider.pipe(map(p => new SharedArray<T>(p?.doc, p?.doc.getArray(name))));
    }

    public getMap<T>(name: string) {
        return this.provider.pipe(map(p => new SharedMap<T>(p?.doc, p?.doc.getMap(name))));
    }

    public updateAwareness(key: string, value: any) {
        this.provider.value?.awareness.setLocalStateField(key, value);
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
        public readonly doc: Y.Doc | undefined,
        public readonly source: Y.Map<T> | undefined,
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
        public readonly doc: Y.Doc | undefined,
        public readonly source: Y.Array<T> | undefined,
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