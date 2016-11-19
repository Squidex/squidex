/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

export const LocalStoreServiceFactory = () => {
    return new LocalStoreService();
};

@Ng2.Injectable()
export class LocalStoreService {
    private readonly fallback = {};
    private store: any = localStorage;

    public configureStore(store: any) {
        this.store = store;
    }

    public get(key: string): any {
        try {
            return this.store.getItem(key);
        } catch (e) {
            return this.fallback[key];
        }
    }

    public set(key: string, value: any) {
        try {
            this.store.setItem(key, value);
        } catch (e) {
            this.fallback[key] = value;
        }
    }
}