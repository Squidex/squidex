/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Immutable from 'immutable';

export class ImmutableSet<T> {
    private readonly items: Immutable.Set<T>;

    public get size(): number {
        return this.items.size;
    }

    constructor(items?: T[] | Immutable.Set<T>) {
        if (Array.isArray(items)) {
            this.items = Immutable.Set<T>(items);
        } else {
            this.items = items || Immutable.Set<T>();
        }

        Object.freeze(this);
    }

    public contains(item: T): boolean {
        return this.items.contains(item);
    }

    public toArray(): T[] {
        return this.items.toArray();
    }

    public map<R>(projection: (item: T) => R): R[] {
        return this.items.map(v => projection(v!)).toArray();
    }

    public filter(projection: (item: T) => boolean): T[] {
        return this.items.filter(v => projection(v!)).toArray();
    }

    public forEach(projection: (item: T) => void): void {
        this.items.forEach(v => projection(v!));
    }

    public add(item: T): ImmutableSet<T> {
        if (!item) {
            return this;
        }

        const newItems = this.items.add(item);

        return this.cloned(newItems);
    }

    public remove(...items: T[]): ImmutableSet<T> {
        for (let item of items) {
            if (!item) {
                return this;
            }
        }

        const newItems = this.items.subtract(items);

        return this.cloned(newItems);
    }

    public set(items: T[]): ImmutableSet<T> {
        if (!items) {
            return this;
        }

        for (let item of items) {
            if (!item) {
                return this;
            }
        }

        const newItems = this.items.intersect(items).merge(items);

        return this.cloned(newItems);
    }

    private cloned(items: Immutable.Set<T>): ImmutableSet<T> {
        if (items !== this.items) {
            return new ImmutableSet<T>(items!);
        } else {
            return this;
        }
    }
}