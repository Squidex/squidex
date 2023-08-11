/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { distinctUntilChanged, map, shareReplay } from 'rxjs/operators';
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

        for (const [key, value] of Object.entries(values)) {
            if (value || !validOnly) {
                clone[key] = value;
            }
        }

        if (Types.isFunction(clone.onCloned)) {
            clone.onCloned();
        }

        return clone;
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

export interface LoadingState {
    // True if currently loading.
    isLoading?: boolean;

    // True if already loaded.
    isLoaded?: boolean;
}

export interface ListState<TQuery = any> extends LoadingState {
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

interface Change<T> {
    // The actual value.
    snapshot: Readonly<T>;

    // The name of the event.
    event: string;
}

export class State<T extends {}> {
    private readonly state: BehaviorSubject<Change<T>>;

    public get changes(): Observable<Change<T>> {
        return this.state;
    }

    public get snapshot(): Readonly<T> {
        return this.state.value.snapshot;
    }

    public project<M>(project: (value: T) => M, compare?: (x: M, y: M) => boolean) {
        return this.changes.pipe(
            map(x => project(x.snapshot)), distinctUntilChanged(compare), shareReplay(1));
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
    ) {
        this.state = new BehaviorSubject({ snapshot: initialState, event: 'Initial' });
    }

    public resetState(update?: ((v: T) => Readonly<T>) | Partial<T> | string, event = 'Reset') {
        if (Types.isString(update)) {
            return this.updateState(this.initialState, event, {});
        } else {
            return this.updateState(this.initialState, event, update);
        }
    }

    public next(update: ((v: T) => Readonly<T>) | Partial<T>, event = 'Update') {
        return this.updateState(this.state.value.snapshot, event, update);
    }

    private updateState(state: T, event: string, update?: ((v: T) => Readonly<T>) | Partial<T>) {
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
                if ((newState as any)[key] !== (this.snapshot as any)[key]) {
                    isChanged = true;
                    break;
                }
            }
        }

        if (isChanged) {
            this.state.next({ snapshot: newState, event });
        }

        return isChanged;
    }
}

const devToolsExtension = (window as any)['__REDUX_DEVTOOLS_EXTENSION__'];

class Connector {
    public static readonly INSTANCE = new Connector();

    private readonly devTools?: any;
    private readonly state: Record<string, any> = {};

    private constructor() {
        if (!devToolsExtension) {
            return;
        }

        this.devTools = devToolsExtension.connect({ name: 'Squidex' });
        this.devTools.init(this.state);
    }

    public connect<T extends {}>(state: State<T>, slice: string) {
        if (!this.devTools) {
            return;
        }

        state.changes
            .subscribe(change => {
                const eventName = `${slice} - ${change.event}`;

                if (this.devTools) {
                    this.state[slice] = change.snapshot;

                    this.devTools?.send(eventName, this.state);
                }
            });
    }
}

export function debug<T extends {}>(state: State<T>, slice: string) {
    Connector.INSTANCE.connect(state, slice);
}