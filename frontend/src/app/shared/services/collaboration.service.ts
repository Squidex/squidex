/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { HocuspocusProvider } from '@hocuspocus/provider';
import { BehaviorSubject, map, Observable, of, switchMap } from 'rxjs';
import * as Y from 'yjs';
import { AuthService, Profile } from './auth.service';

@Injectable()
export class CollaborationService {
    private readonly provider = new BehaviorSubject<HocuspocusProvider | null>(null);
    private previousName?: string | null;

    public awareness =
        this.provider.pipe(map(x => x?.awareness));

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
                            if (Object.keys(state).length > 0) {
                                result.push(state as Profile);
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

    constructor(
        private readonly authService: AuthService,
    ) {
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

        const provider = new HocuspocusProvider({
            url: 'ws://localhost:8080/collaboration',
            name,
            token: 'Adasd',
        });

        const user = this.authService.user!;

        if (user) {
            const { id, email, displayName } = user;

            provider.awareness?.setLocalState({ id, email, displayName });
        }

        this.provider.next(provider);
    }

    private cleanup() {
        this.provider.value?.destroy();
        this.provider.next(null);
    }

    public getArray<T>(name: string) {
        return this.provider.pipe(map(p => new Array<T>(p?.document.getArray(name))));
    }

    public getMap<T>(name: string) {
        return this.provider.pipe(map(p => new Map<T>(p?.document.getMap(name))));
    }
}

class Map<T> {
    public readonly values: BehaviorSubject<Record<string, T>>;

    constructor(
        private readonly source: Y.Map<T> | undefined,
    ) {
        this.values = new BehaviorSubject(source?.toJSON() || {});

        source?.observeDeep(() => {
            this.values.next(source?.toJSON());
        });
    }

    public set(key: string, value: T) {
        this.source?.set(key, value);
    }

    public remove(key: string) {
        this.source?.delete(key);
    }
}

class Array<T> {
    public readonly values: BehaviorSubject<T[]>;

    constructor(
        private readonly source: Y.Array<T> | undefined,
    ) {
        this.values = new BehaviorSubject(source?.toJSON() || []);

        source?.observeDeep(() => {
            this.values.next(source.toJSON());
        });
    }

    public add(value: T) {
        this.source?.insert(this.source.length, [value]);
    }

    public remove(index: number) {
        this.source?.delete(index, 1);
    }

    public set(index: number, value: T) {
        this.source?.delete(index, 1);
        this.source?.insert(index, [value]);
    }
}