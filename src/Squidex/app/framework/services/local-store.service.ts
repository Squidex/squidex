/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';

export const LocalStoreServiceFactory = () => {
    return new LocalStoreService();
};

@Injectable()
export class LocalStoreService {
    private readonly fallback = {};
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

    public set(key: string, value: string) {
        try {
            this.store.setItem(key, value);
        } catch (e) {
            this.fallback[key] = value;
        }
    }
}