/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { distinctUntilChanged, map, shareReplay } from 'rxjs/operators';
import { ResourceLinks } from './utils/hateos';
import { Types } from './utils/types';

export type Mutable<T> = {
    -readonly [P in keyof T ]: T[P]
};

export class Model<T> {
    public with(value: Partial<T>, validOnly = false): T {
        return this.clone(value, validOnly);
    }

    protected clone<V>(update: ((v: any) => V) | Partial<V>, validOnly = false): V {
        let values: Partial<V>;

        if (Types.isFunction(update)) {
            values = update(this as any);
        } else {
            values = update;
        }

        const clone = Object.assign(Object.create(Object.getPrototypeOf(this)), this);

        for (const key in values) {
            if (values.hasOwnProperty(key)) {
                const value = values[key];

                if (value || !validOnly) {
                    clone[key] = value;
                }
            }
        }

        if (Types.isFunction(clone.onCloned)) {
            clone.onCloned();
        }

        return clone;
    }
}

export class ResultSet<T> {
    public readonly _links: ResourceLinks;

    constructor(
        public readonly total: number,
        public readonly items: ReadonlyArray<T>,
        links?: ResourceLinks,
    ) {
        this._links = links || {};
    }
}

export interface PagingInfo {
    // The current page.
    page: number;

    // The current page size.
    pageSize: number;

    // The total number of items.
    total: number;

    // The current number of items.
    count: number;
}

export function getPagingInfo<T>(state: ListState<T>, count: number) {
    const { page, pageSize, total } = state;

    return { page, pageSize, total, count };
}

export interface ListState<TQuery = any> {
    // The total number of items.
    total: number;

    // True if currently loading.
    isLoading?: boolean;

    // True if already loaded.
    isLoaded?: boolean;

    // The current page.
    page: number;

    // The current page size.
    pageSize: number;

    // The query.
    query?: TQuery;
}

const devToolsExtension = window['__REDUX_DEVTOOLS_EXTENSION__'];

export class State<T extends {}> {
    private readonly state: BehaviorSubject<Readonly<T>>;
    private readonly devTools?: any;

    public get changes(): Observable<Readonly<T>> {
        return this.state;
    }

    public get snapshot(): Readonly<T> {
        return this.state.value;
    }

    public project<M>(project: (value: T) => M, compare?: (x: M, y: M) => boolean) {
        return this.changes.pipe(
            map(project), distinctUntilChanged(compare), shareReplay(1));
    }

    public projectFrom<M, N>(source: Observable<M>, project: (value: M) => N, compare?: (x: N, y: N) => boolean) {
        return source.pipe(
            map(project), distinctUntilChanged(compare), shareReplay(1));
    }

    public projectFrom2<M, N, O>(lhs: Observable<M>, rhs: Observable<N>, project: (l: M, r: N) => O, compare?: (x: O, y: O) => boolean) {
        return combineLatest([lhs, rhs]).pipe(
            map(([x, y]) => project(x, y)), distinctUntilChanged(compare), shareReplay(1));
    }

    constructor(
        private readonly initialState: Readonly<T>,
        private readonly debugName?: string,
    ) {
        this.state = new BehaviorSubject(initialState);

        if (debugName && devToolsExtension) {
            const name = `[Squidex] ${debugName}`;

            this.devTools = devToolsExtension.connect({ name, features: {} });
            this.devTools.init(initialState);
        }
    }

    public resetState(update?: ((v: T) => Readonly<T>) | Partial<T> | string, action = 'Reset') {
        if (Types.isString(update)) {
            return this.updateState(this.initialState, {}, update);
        } else {
            return this.updateState(this.initialState, update, action);
        }
    }

    public next(update: ((v: T) => Readonly<T>) | Partial<T>, action = 'Update') {
        return this.updateState(this.state.value, update, action);
    }

    private updateState(state: T, update?: ((v: T) => Readonly<T>) | Partial<T>, action?: string) {
        let newState = state;

        if (update) {
            if (Types.isFunction(update)) {
                newState = update(state);
            } else {
                newState = { ...state, ...update };
            }
        }

        let isChanged = false;

        const newKeys = Object.keys(newState);

        if (newKeys.length !== Object.keys(this.snapshot).length) {
            isChanged = true;
        } else {
            for (const key of newKeys) {
                if (newState[key] !== this.snapshot[key]) {
                    isChanged = true;
                    break;
                }
            }
        }

        if (isChanged) {
            if (action && this.devTools) {
                const name = `[${this.debugName}] ${action}`;

                this.devTools?.send(name, newState);
            }

            this.state.next(newState);
        }

        return isChanged;
    }
}
