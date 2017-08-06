/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

interface Entry { value: any, expires: number };

export const LocalCacheServiceFactory = () => {
    return new LocalCacheService();
};

export class LocalCacheService {
    private readonly entries: { [key: string]: Entry } = {};

    public clear(force: boolean) {
        const now = new Date().getTime();

        for (let key in this.entries) {
            if (this.entries.hasOwnProperty(key)) {
                const entry = this.entries[key];

                if (force || LocalCacheService.isExpired(now, entry)) {
                    delete this.entries[key];
                }
            }
        }
    }

    public get<T>(key: string, now?: number): T {
        const entry = this.entries[key];

        if (entry) {
            now = now || new Date().getTime();

            if (!LocalCacheService.isExpired(now, entry)) {
                delete this.entries[key];

                return <T> entry.value;
            }
        }

        return undefined;
    }

    public set<T>(key: string, value: T, expiresIn = 100) {
        this.entries[key] = { value, expires: new Date().getTime() + expiresIn };
    }

    private static isExpired(now: number, entry: Entry): boolean {
        return entry.expires < now;
    }
}