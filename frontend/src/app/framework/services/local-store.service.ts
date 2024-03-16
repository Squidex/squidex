/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Types } from './../utils/types';

@Injectable()
export class LocalStoreService {
    private readonly fallback: { [key: string]: string } = {};
    private store = localStorage;

    public configureStore(store: any) {
        this.store = store;
    }

    public get(key: string): string | null {
        try {
            return this.store.getItem(key);
        } catch (e) {
            return this.fallback[key];
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
        } catch (e) {
            this.fallback[key] = value;
        }
    }

    public setBoolean(key: string, value: boolean) {
        const converted = value ? 'true' : 'false';

        this.store.setItem(key, converted);
    }

    public setInt(key: string, value: number) {
        const converted = `${value}`;

        this.store.setItem(key, converted);
    }

    public remove(key: string) {
        try {
            this.store.removeItem(key);
        } catch (e) {
            delete this.fallback[key];
        }
    }
}
