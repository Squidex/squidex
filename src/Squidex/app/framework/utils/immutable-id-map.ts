/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Immutable from 'immutable';

export interface WithId {
    id: string;
}

export class ImmutableIdMap<T extends WithId> {
    private readonly items: Immutable.OrderedMap<string, T>;

    public get size(): number {
        return this.items.size;
    }

    public get first(): T {
        return this.items.first();
    }

    public get last(): T {
        return this.items.last();
    }

    constructor(items?: T[] | Immutable.OrderedMap<string, T>) {
        if (Array.isArray(items)) {
            this.items = Immutable.OrderedMap<string, T>(items.map(x => [x.id, x]));
        } else {
            this.items = items || Immutable.OrderedMap<string, T>();
        }

        Object.freeze(this);
    }

    public get(key: string): T | undefined {
        return this.items.get(key, undefined);
    }

    public contains(id: string): boolean {
        return !!this.items.get(id);
    }

    public toArray(): T[] {
        return this.items.toArray();
    }

    public map<R>(projection: (item: T, key?: string) => R): R[] {
        return this.items.map((v, k) => projection(v!, k)).toArray();
    }

    public filter(projection: (item: T, key?: string) => boolean): T[] {
        return this.items.filter((v, k) => projection(v!, k)).toArray();
    }

    public forEach(projection: (item: T, key?: string) => void): void {
        this.items.forEach((v, k) => projection(v!, k));
    }

    public add(...items: T[]): ImmutableIdMap<T> {
        for (let item of items) {
            if (!item || !item.id || this.get(item.id)) {
                return this;
            }
        }

        let newItems = this.items;

        if (items.length > 50) {
            newItems = this.items.withMutations(mutable => {
                for (let item of items) {
                    mutable.set(item.id, item);
                }
            });
        } else {
            for (let item of items) {
                newItems = newItems.set(item.id, item);
            }
        }

        return this.cloned(newItems);
    }

    public remove(...ids: string[]): ImmutableIdMap<T> {
        for (let id of ids) {
            if (!id || !this.get(id)) {
                return this;
            }
        }

        let newItems = this.items;

        if (ids.length > 50) {
            newItems = this.items.withMutations(mutable => {
                for (let id of ids) {
                    mutable.remove(id);
                }
            });
        } else {
            for (let id of ids) {
                newItems = newItems.remove(id);
            }
        }

        return this.cloned(newItems);
    }

    public update(id: string, updater: (item: T) => T): ImmutableIdMap<T> {
        const oldItem = this.items.get(id);

        if (!oldItem || !updater) {
            return this;
        }

        const newItem = updater(oldItem);

        if (!newItem || newItem === oldItem || newItem.id !== oldItem.id) {
            return this;
        }

        const newItems = this.items.set(id, newItem);

        return this.cloned(newItems);
    }

    public bringToFront(ids: string[]): ImmutableIdMap<T> {
        return this.moveTo(ids, Number.MAX_VALUE);
    }

    public bringForwards(ids: string[]): ImmutableIdMap<T> {
        return this.moveTo(ids, 1, true);
    }

    public sendBackwards(ids: string[]): ImmutableIdMap<T> {
        return this.moveTo(ids, -1, true);
    }

    public sendToBack(ids: string[]): ImmutableIdMap<T> {
        return this.moveTo(ids, 0);
    }

    public moveTo(ids: string[], target: number, relative = false): ImmutableIdMap<T> {
        const itemsToStay: ItemToSort<T>[] = [];
        const itemsToMove: ItemToSort<T>[] = [];

        this.items.toArray().forEach((item: T, index: number) => {
            const itemToAdd: ItemToSort<T> = { isInIds: ids && ids.indexOf(item.id) >= 0, index: index, value: item };

            if (itemToAdd.isInIds) {
                itemsToMove.push(itemToAdd);
            } else {
                itemsToStay.push(itemToAdd);
            }
        });

        if (itemsToMove.length === 0) {
            return this;
        }

        let isBackwards = false, newIndex = 0;

        if (relative) {
            isBackwards = target <= 0;

            let currentIndex =
                target > 0 ?
                    Number.MIN_VALUE :
                    Number.MAX_VALUE;

            for (let itemFromIds of itemsToMove) {
                if (target > 0) {
                    currentIndex = Math.max(itemFromIds.index, currentIndex);
                } else {
                    currentIndex = Math.min(itemFromIds.index, currentIndex);
                }
            }

            newIndex = currentIndex + target;
        } else {
            newIndex = target;

            if (itemsToMove[0].index > newIndex) {
                isBackwards = true;
            }
        }

        const result: any[][] = [];

        for (let item of itemsToStay) {
            if ((isBackwards && item.index >= newIndex) || item.index > newIndex) {
                break;
            }

            result.push([item.value.id, item.value]);
        }

        for (let item of itemsToMove) {
            result.push([item.value.id, item.value]);
        }

        for (let item of itemsToStay) {
            if ((isBackwards && item.index >= newIndex) || item.index > newIndex) {
                result.push([item.value.id, item.value]);
            }
        }

        return this.cloned(Immutable.OrderedMap<string, T>(result));
    }

    private cloned(items: Immutable.OrderedMap<string, T>): ImmutableIdMap<T> {
        return new ImmutableIdMap<T>(items);
    }
}

interface ItemToSort<T> { isInIds: boolean; index: number; value: T; }