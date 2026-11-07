/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Injectable, signal, untracked, WritableSignal } from '@angular/core';
import { Types } from '../utils/types';

@Injectable({
    providedIn: 'root',
})
export class LocalStoreService {
    private readonly fallback: { [key: string]: string } = {};
    private readonly signals = new Map<string, WritableSignal<string | null>>();
    private store = localStorage;

    public configureStore(store: any) {
        this.store = store;
    }

    public value(key: string): WritableSignal<string | null> {
        return this.getOrCreate(key);
    }

    public boolean(key: string): WritableSignal<boolean> {
        return mapped(this.getOrCreate(key), x => x === 'true', x => (x ? 'true' : 'false'));
    }

    public int(key: string, fallback = 0): WritableSignal<number> {
        return mapped(this.getOrCreate(key), x => parseInt(x, fallback), x => `${x}`);
    }

    public get(key: string): string | null {
        return untracked(this.getOrCreate(key));
    }

    public getBoolean(key: string): boolean {
        return untracked(this.boolean(key));
    }

    public getInt(key: string, fallback = 0): number {
        return untracked(this.int(key, fallback));
    }

    public set(key: string, value: string) {
        this.getOrCreate(key).set(value);
    }

    public setBoolean(key: string, value: boolean) {
        this.boolean(key).set(value);
    }

    public setInt(key: string, value: number) {
        this.int(key).set(value);
    }

    public remove(key: string) {
        this.getOrCreate(key).set(null);
    }

    private getOrCreate(key: string) {
        let result = this.signals.get(key);
        if (!result) {
            result = signal(this.read(key));

            // Every write goes to the store, so all signals for the same key stay in sync.
            const set = result.set.bind(result);

            result.set = value => {
                this.write(key, value);
                set(value);
            };

            result.update = fn => result!.set(fn(untracked(result!)));

            this.signals.set(key, result);
        }

        return result;
    }

    private read(key: string): string | null {
        try {
            return this.store.getItem(key);
        } catch {
            return this.fallback[key] ?? null;
        }
    }

    private write(key: string, value: string | null) {
        try {
            if (value === null) {
                this.store.removeItem(key);
            } else {
                this.store.setItem(key, value);
            }
        } catch {
            if (value === null) {
                delete this.fallback[key];
            } else {
                this.fallback[key] = value;
            }
        }
    }
}

function parseInt(value: string | null, fallback: number) {
    if (!Types.isString(value)) {
        return fallback;
    }

    const result = Number.parseInt(value, 10);

    return Types.isNumber(result) && !Number.isNaN(result) ? result : fallback;
}

function mapped<T>(source: WritableSignal<string | null>, read: (value: string | null) => T, write: (value: T) => string): WritableSignal<T> {
    const result = computed(() => read(source())) as unknown as WritableSignal<T>;

    result.set = value => source.set(write(value));
    result.update = fn => result.set(fn(untracked(result)));
    result.asReadonly = () => result;

    return result;
}
