/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, inject, Injectable, Signal, signal, untracked, WritableSignal } from '@angular/core';
import { Awareness } from 'y-protocols/awareness';
import { WebsocketProvider } from 'y-websocket';
import * as Y from 'yjs';
import { Types, UIOptions } from '@app/framework';
import { AuthService, Profile } from './auth.service';

type AwarenessState = { user: Profile; [key: string]: any };

export type CollaborationProvider = { awareness: Awareness; doc: Y.Doc; destroy: () => void };

@Injectable()
export class CollaborationService {
    private readonly provider = signal<CollaborationProvider | null>(null);
    private readonly uiOptions = inject(UIOptions);
    private readonly basePath = this.uiOptions.value.collaborationService;
    private readonly profile: Profile;
    private readonly usersState = signal<AwarenessState[]>([], { equal: Types.equals });
    private previousName?: string | null;
    private unlistenAwareness?: () => void;

    public readonly users = this.usersState.asReadonly();

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

        this.provider.set(yProvider);
        this.listenAwareness(yProvider.awareness);
    }

    private cleanup() {
        this.unlistenAwareness?.();
        this.unlistenAwareness = undefined;

        untracked(this.provider)?.destroy();
        this.provider.set(null);
        this.usersState.set([]);
    }

    private listenAwareness(awareness: Awareness | undefined) {
        if (!awareness) {
            return;
        }

        const updateUsers = () => {
            const byUser: { [userId: string]: AwarenessState } = {};

            awareness.getStates().forEach((state, clientId) => {
                if (clientId == awareness.clientID) {
                    return;
                }

                if (!state.user || state.user.id === this.profile.id) {
                    return;
                }

                byUser[state.user.id] = state as any;
            });

            const sorted = Object.values(byUser).sortByString(x => x.user.displayName);

            this.usersState.set(sorted);
        };

        updateUsers();
        awareness.on('change', updateUsers);

        this.unlistenAwareness = () => awareness.off('change', updateUsers);
    }

    public getArray<T>(name: string): Signal<SharedArray<T>> {
        return computed(() => {
            const provider = this.provider();

            return new SharedArray<T>(provider?.doc, provider?.doc.getArray(name));
        });
    }

    public getMap<T>(name: string): Signal<SharedMap<T>> {
        return computed(() => {
            const provider = this.provider();

            return new SharedMap<T>(provider?.doc, provider?.doc.getMap(name));
        });
    }

    public updateAwareness(key: string, value: any) {
        untracked(this.provider)?.awareness.setLocalStateField(key, value);
    }
}

export class SharedMap<T> {
    private readonly valueState: WritableSignal<Readonly<Record<string, T>>>;

    public readonly value: Signal<Readonly<Record<string, T>>>;

    constructor(
        public readonly doc: Y.Doc | undefined,
        public readonly source: Y.Map<T> | undefined,
    ) {
        this.valueState = signal(source?.toJSON() || {});
        this.value = this.valueState.asReadonly();

        source?.observeDeep(() => {
            this.valueState.set(source.toJSON());
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
    private readonly itemsState: WritableSignal<ReadonlyArray<T>>;

    public readonly items: Signal<ReadonlyArray<T>>;

    constructor(
        public readonly doc: Y.Doc | undefined,
        public readonly source: Y.Array<T> | undefined,
    ) {
        this.itemsState = signal(source?.toJSON() || []);
        this.items = this.itemsState.asReadonly();

        source?.observeDeep(() => {
            this.itemsState.set(source.toJSON() || []);
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
