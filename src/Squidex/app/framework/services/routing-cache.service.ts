/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class RoutingCache {
    private readonly entries: { [key: string]: { value: any, inserted: number } } = {};

    public getValue<T>(key: string) {
        let entry = this.entries[key];

        if (entry && (new Date().getTime() - entry.inserted) < 100) {
            return <T> entry.value;
        } else {
            return undefined;
        }
    }

    public set<T>(key: string, value: T) {
        this.entries[key] = { value, inserted: new Date().getTime() };
    }
}