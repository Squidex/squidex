/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

export const LocalStoreServiceFactory = () => {
    return new LocalStoreService();
};

@Injectable()
export class LocalStoreService {
    private readonly fallback: { [key: string]: string } = {};
    private store: any = localStorage;

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

        return value ? (parseInt(value, 10) || fallback) : fallback;
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
}