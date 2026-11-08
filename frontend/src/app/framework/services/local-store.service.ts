/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Types } from '../utils/types';

@Injectable({
    providedIn: 'root',
})
export class LocalStoreService {
    private readonly fallback: { [key: string]: string } = {};
    private store = localStorage;

    public configureStore(store: any) {
        this.store = store;
    }

    public get(key: string): string | null {
        try {
            return this.store.getItem(key);
        } catch {
            return this.fallback[key] ?? null;
        }
    }

    public getBoolean(key: string): boolean {
        const value = this.get(key);

        return value === 'true';
    }

    public getInt(key: string, fallback = 0): number {
        const value = this.get(key);

        let result = fallback;

        if (Types.isString(value)) {
            result = parseInt(value, 10);
        }

        if (!Types.isNumber(result)) {
            result = fallback;
        }

        return result;
    }

    public set(key: string, value: string) {
        try {
            this.store.setItem(key, value);
        } catch {
            this.fallback[key] = value;
        }
    }

    public setBoolean(key: string, value: boolean) {
        this.set(key, value ? 'true' : 'false');
    }

    public setInt(key: string, value: number) {
        this.set(key, `${value}`);
    }

    public remove(key: string) {
        try {
            this.store.removeItem(key);
        } catch {
            delete this.fallback[key];
        }
    }
}
