/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export interface IdField {
    id: string;
}

function freeze<T>(items: T[]): T[] {
    for (let item of items) {
        Object.freeze(item);
    }

    return items;
}

export class ImmutableArray<T> implements Iterable<T> {
    private static readonly EMPTY = new ImmutableArray<any>([]);
    private readonly items: T[];

    public [Symbol.iterator](): Iterator<T> {
        return this.items.values();
    }

    public get length(): number {
        return this.items.length;
    }

    public get values(): T[] {
        return [...this.items];
    }

    public get mutableValues(): T[] {
        return this.items;
    }

    private constructor(items: T[]) {
        this.items = items;
    }

    public static empty<V>(): ImmutableArray<V> {
        return ImmutableArray.EMPTY;
    }

    public static of<V>(items?: V[]): ImmutableArray<V> {
        if (!items || items.length === 0) {
            return ImmutableArray.EMPTY;
        } else {
            return new ImmutableArray<V>(freeze([...items]));
        }
    }

    public map<R>(projection: (item: T) => R): ImmutableArray<R> {
        return new ImmutableArray<R>(freeze(this.items.map(v => projection(v!))));
    }

    public filter(predicate: (item: T) => boolean): ImmutableArray<T> {
        return new ImmutableArray<T>(this.items.filter(v => predicate(v!)));
    }

    public find(predicate: (item: T, index: number) => boolean): T {
        return this.items.find(predicate);
    }

    public sort(compareFn?: (a: T, b: T) => number): ImmutableArray<T> {
        const clone = [...this.items];

        clone.sort(compareFn);

        return new ImmutableArray<T>(clone);
    }

    public sortByStringAsc(filter: (a: T) => string): ImmutableArray<T> {
        return this.sort((a, b) => {
            const av = filter(a);
            const bv = filter(b);

            if (av < bv) {
                return -1;
            }
            if (av > bv) {
                return 1;
            }
            return 0;
        });
    }

    public sortByStringDesc(filter: (a: T) => string): ImmutableArray<T> {
        return this.sort((a, b) => {
            const av = filter(a);
            const bv = filter(b);

            if (av < bv) {
                return 1;
            }
            if (av > bv) {
                return -1;
            }
            return 0;
        });
    }

    public sortByNumberAsc(filter: (a: T) => number): ImmutableArray<T> {
        return this.sort((a, b) => {
            const av = filter(a);
            const bv = filter(b);

            if (av < bv) {
                return -1;
            }
            if (av > bv) {
                return 1;
            }
            return 0;
        });
    }

    public sortByNumberDesc(filter: (a: T) => number): ImmutableArray<T> {
        return this.sort((a, b) => {
            const av = filter(a);
            const bv = filter(b);

            if (av < bv) {
                return 1;
            }
            if (av > bv) {
                return -1;
            }
            return 0;
        });
    }

    public pushFront(...items: T[]): ImmutableArray<T> {
        if (!items || items.length === 0) {
            return this;
        }
        return new ImmutableArray<T>([...freeze(items), ...this.items]);
    }

    public push(...items: T[]): ImmutableArray<T> {
        if (!items || items.length === 0) {
            return this;
        }
        return new ImmutableArray<T>([...this.items, ...freeze(items)]);
    }

    public remove(...items: T[]): ImmutableArray<T> {
        if (!items || items.length === 0) {
            return this;
        }

        const copy = this.items.slice();

        for (let item of items) {
            const index = copy.indexOf(item);

            if (index >= 0) {
                copy.splice(index, 1);
            }
        }

        return new ImmutableArray<T>(copy);
    }

    public removeAll(predicate: (item: T, index: number) => boolean): ImmutableArray<T> {
        const copy = this.items.slice();

        let hasChange = false;

        for (let i = 0; i < copy.length; ) {
            if (predicate(copy[i], i)) {
                copy.splice(i, 1);

                hasChange = true;
            } else {
                ++i;
            }
        }

        return hasChange ? new ImmutableArray<T>(copy) : this;
    }

    public replace(oldItem: T, newItem: T): ImmutableArray<T> {
        const index = this.items.indexOf(oldItem);

        if (index >= 0) {
            if (newItem) {
                Object.freeze(newItem);
            }

            const copy = [...this.items.slice(0, index), newItem, ...this.items.slice(index + 1)];

            return new ImmutableArray<T>(copy);
        } else {
            return this;
        }
    }

    public replaceAll(predicate: (item: T, index: number) => boolean, replacer: (item: T) => T): ImmutableArray<T> {
        const copy = this.items.slice();

        let hasChange = false;

        for (let i = 0; i < copy.length; i++) {
            if (predicate(copy[i], i)) {
                const newItem = replacer(copy[i]);

                if (newItem) {
                    Object.freeze(newItem);
                }

                if (copy[i] !== newItem) {
                    copy[i] = newItem;

                    hasChange = true;
                }
            }
        }

        return hasChange ? new ImmutableArray<T>(copy) : this;
    }

    public replaceBy(field: string, newValue: T, replacer?: (o: T, n: T) => T) {
        return this.replaceAll(x => x[field] === newValue[field], o => replacer ? replacer(o, newValue) : newValue);
    }
}