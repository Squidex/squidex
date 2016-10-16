/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Immutable from 'immutable';

export class ImmutableList<T> {
    private readonly items: Immutable.List<T>;

    public get size(): number {
        return this.items.size;
    }

    constructor(items?: T[] | Immutable.List<T>) {
        if (Array.isArray(items)) {
            this.items = Immutable.List<T>(items);
        } else {
            this.items = items || Immutable.List<T>();
        }

        Object.freeze(this);
    }

    public get(index: number): T | undefined {
        return this.items.get(index, undefined);
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

    public add(...items: T[]): ImmutableList<T> {
        for (let item of items) {
            if (!item) {
                return this;
            }
        }

        let newItems = this.items;

        if (items.length > 50) {
            newItems = this.items.withMutations(mutable => {
                for (let item of items) {
                    mutable.push(item);
                }
            });
        } else {
            for (let item of items) {
                newItems = newItems.push(item);
            }
        }

        return this.cloned(newItems);
    }

    public remove(...items: T[]): ImmutableList<T> {
        for (let item of items) {
            if (!item || this.items.indexOf(item) < 0) {
                return this;
            }
        }

        let newItems = this.items;

        if (items.length > 50) {
            newItems = this.items.withMutations(mutable => {
                for (let item of items) {
                    mutable.remove(mutable.indexOf(item));
                }
            });
        } else {
            for (let item of items) {
                newItems = newItems.remove(newItems.indexOf(item));
            }
        }

        return this.cloned(newItems);
    }

    public update(item: T, updater: (item: T) => T): ImmutableList<T> {
        const index = this.items.indexOf(item);

        if (index < 0 || !updater) {
            return this;
        }

        const newItem = updater(item);

        if (!newItem || newItem === item) {
            return this;
        }

        const newItems = this.items.set(index, newItem);

        return this.cloned(newItems);
    }

    public bringToFront(items: T[]): ImmutableList<T> {
        return this.moveTo(items, Number.MAX_VALUE);
    }

    public bringForwards(items: T[]): ImmutableList<T> {
        return this.moveTo(items, 1, true);
    }

    public sendBackwards(items: T[]): ImmutableList<T> {
        return this.moveTo(items, -1, true);
    }

    public sendToBack(items: T[]): ImmutableList<T> {
        return this.moveTo(items, 0);
    }

    public moveTo(items: T[], target: number, relative = false): ImmutableList<T> {
        const itemsToStay: ItemToSort<T>[] = [];
        const itemsToMove: ItemToSort<T>[] = [];

        const allItems = this.items.toArray();

        for (let i = 0; i < allItems.length; i++) {
            const item = allItems[i];

            const itemToAdd: ItemToSort<T> = { isInItems: items && items.indexOf(item) >= 0, index: i, value: item };

            if (itemToAdd.isInItems) {
                itemsToMove.push(itemToAdd);
            } else {
                itemsToStay.push(itemToAdd);
            }
        }

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

        const result: T[] = [];

        for (let item of itemsToStay) {
            if ((isBackwards && item.index >= newIndex) || item.index > newIndex) {
                break;
            }

            result.push(item.value);
        }

        for (let item of itemsToMove) {
            result.push(item.value);
        }

        for (let item of itemsToStay) {
            if ((isBackwards && item.index >= newIndex) || item.index > newIndex) {
                result.push(item.value);
            }
        }

        return new ImmutableList<T>(result);
    }

    private cloned(items: Immutable.List<T>): ImmutableList<T> {
        return new ImmutableList<T>(items);
    }
}

interface ItemToSort<T> { isInItems: boolean; index: number; value: T; }