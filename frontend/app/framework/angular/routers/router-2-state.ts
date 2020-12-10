/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: forin
// tslint:disable: readonly-array

import { Injectable, OnDestroy } from '@angular/core';
import { ActivatedRoute, NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Params, Router } from '@angular/router';
import { LocalStoreService, Types } from '@app/framework/internal';
import { State } from '@app/framework/state';
import { Subscription } from 'rxjs';

export type QueryParams = { [name: string]: string };

export interface RouteSynchronizer {
    readonly keys: ReadonlyArray<string>;

    parseFromRoute(query: QueryParams): object | undefined;

    parseFromState(state: any): QueryParams | undefined;
}

export class PagingSynchronizer implements RouteSynchronizer {
    public readonly keys = ['page', 'pageSize'];

    constructor(
        private readonly localStore: LocalStoreService,
        private readonly storeName: string,
        private readonly defaultSize: number
    ) {
    }

    public parseFromRoute(query: QueryParams) {
        let pageSize = 0;

        const pageSizeValue = query['pageSize'];

        if (Types.isString(pageSizeValue)) {
            pageSize = parseInt(pageSizeValue, 10);
        }

        if (pageSize <= 0 || pageSize > 100 || isNaN(pageSize)) {
            pageSize = this.localStore.getInt(`${this.storeName}.pageSize`, this.defaultSize);
        }

        if (pageSize <= 0 || pageSize > 100 || isNaN(pageSize)) {
            pageSize = this.defaultSize;
        }

        let page = parseInt(query['page'], 10);

        if (page <= 0 || isNaN(page)) {
            page = 0;
        }

        return { page, pageSize };
    }

    public parseFromState(state: any) {
        let page = undefined;

        const pageSize: number = state.pageSize;

        if (state.page > 0) {
            page = state.page.toString();
        }

        this.localStore.setInt(`${this.storeName}.pageSize`, pageSize);

        return { page, pageSize: pageSize.toString() };
    }
}

export class StringSynchronizer implements RouteSynchronizer {
    public get keys() {
        return [this.key];
    }

    constructor(
        private readonly key: string
    ) {
    }

    public parseFromRoute(params: QueryParams) {
        const value = params[this.key];

        return { [this.key]: value };
    }

    public parseFromState(state: any) {
        const value = state[this.key];

        if (Types.isString(value)) {
            return { [this.key]: value };
        }
    }
}

export class StringKeysSynchronizer implements RouteSynchronizer {
    public get keys() {
        return [this.key];
    }

    constructor(
        private readonly key: string
    ) {
    }

    public parseFromRoute(query: QueryParams) {
        const value = query[this.key];

        const result: { [key: string]: boolean } = {};

        if (Types.isString(value)) {
            for (const item of value.split(',')) {
                if (item.length > 0) {
                    result[item] = true;
                }
            }
        }

        return { [this.key]: result };
    }

    public parseFromState(state: any) {
        const value = state[this.key];

        if (Types.isObject(value)) {
            const items = Object.keys(value).join(',');

            if (items.length > 0) {
                return { [this.key]: items };
            }
        }
    }
}

export interface StateSynchronizer {
    mapTo<T extends object>(state: State<T>): StateSynchronizerMap<T>;
}

export interface StateSynchronizerMap<T> {
    keep(key: keyof T & string): this;

    withString(key: keyof T & string): this;

    withStrings(key: keyof T & string): this;

    withPaging(storeName: string, defaultSize: number): this;

    whenSynced(action: () => void): this;

    withSynchronizer(synchronizer: RouteSynchronizer): this;

    build(): void;
}

@Injectable()
export class Router2State implements OnDestroy, StateSynchronizer {
    private mapper: Router2StateMap<any>;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly localStore: LocalStoreService
    ) {
    }

    public ngOnDestroy() {
        this.mapper?.destroy();
    }

    public mapTo<T extends object>(state: State<T>) {
        this.mapper?.destroy();
        this.mapper = this.mapper || new Router2StateMap<T>(state, this.route, this.router, this.localStore);

        return this.mapper;
    }
}

export class Router2StateMap<T extends object> implements StateSynchronizerMap<T> {
    private readonly syncs: RouteSynchronizer[] = [];
    private readonly keysToKeep: string[] = [];
    private syncDone: (() => void)[] = [];
    private lastSyncedQuery?: Params;
    private lastSyncedState?: Partial<T>;
    private subscriptionChanges: Subscription;
    private subscriptionQueryParams: Subscription;
    private subscriptionEvents: Subscription;
    private isNavigating = false;
    private pendingParams?: Params;

    constructor(
        private readonly state: State<T>,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly localStore: LocalStoreService
    ) {
    }

    public build() {
        this.subscriptionQueryParams =
            this.route.queryParams
                .subscribe(q =>  this.syncFromRoute(q));

        this.subscriptionChanges =
            this.state.changes
                .subscribe(s => this.syncToRoute(s));

        this.subscriptionEvents =
            this.router.events
                .subscribe(event => {
                    if (Types.is(event, NavigationStart)) {
                        this.isNavigating = true;
                    } else if (
                        Types.is(event, NavigationEnd) ||
                        Types.is(event, NavigationCancel) ||
                        Types.is(event, NavigationError)) {
                        this.isNavigating = false;

                        if (this.pendingParams) {
                            this.syncFromParams(this.pendingParams);
                        }
                    }
                });

        this.lastSyncedQuery = undefined;
        this.lastSyncedState = undefined;
    }

    public destroy() {
        this.syncDone = [];

        this.subscriptionQueryParams?.unsubscribe();
        this.subscriptionChanges?.unsubscribe();
        this.subscriptionEvents?.unsubscribe();
    }

    private syncToRoute(state: T) {
        if (!isChanged(this.syncs, state, this.lastSyncedState)) {
            return;
        }

        const query: Params = {};

        for (const sync of this.syncs) {
            const values = sync.parseFromState(state);

            for (const key of sync.keys) {
                query[key] = values?.[key];
            }
        }

        if (this.isNavigating) {
            this.pendingParams = query;
        } else {
            this.syncFromParams(query);
        }
    }

    private syncFromParams(query: Params) {
        this.pendingParams = undefined;

        if (!isChanged(this.syncs, query, this.lastSyncedQuery)) {
            return;
        }

        this.router.navigate([], {
            queryParams: query,
            queryParamsHandling: 'merge',
            replaceUrl: true
        });

        this.lastSyncedQuery = query;
    }

    private syncFromRoute(query: Params) {
        if (!isChanged(this.syncs, query, this.lastSyncedQuery)) {
            return;
        }

        const update: Partial<T> = {};

        for (const sync of this.syncs) {
            const values = sync.parseFromRoute(query);

            for (const key of sync.keys) {
                update[key] = values?.[key];
            }
        }

        for (const key of this.keysToKeep) {
            update[key] = this.state.snapshot[key];
        }

        if (!isChanged(this.syncs, update, this.lastSyncedState)) {
            return;
        }

        if (this.state.resetState(update)) {
            for (const action of this.syncDone) {
                action();
            }
        }

        this.lastSyncedState = update;
    }

    public keep(key: keyof T & string) {
        this.keysToKeep.push(key);

        return this;
    }

    public withString(key: keyof T & string) {
        return this.withSynchronizer(new StringSynchronizer(key));
    }

    public withStrings(key: keyof T & string) {
        return this.withSynchronizer(new StringKeysSynchronizer(key));
    }

    public withPaging(storeName: string, defaultSize = 10) {
        return this.withSynchronizer(new PagingSynchronizer(this.localStore, storeName, defaultSize));
    }

    public withSynchronizer(synchronizer: RouteSynchronizer) {
        this.syncs.push(synchronizer);

        return this;
    }

    public whenSynced(action: () => void) {
        this.syncDone.push(action);

        return this;
    }
}

function isChanged(syncs: ReadonlyArray<RouteSynchronizer>, next: object, prev?: object) {
    if (!prev) {
        return true;
    }

    for (const sync of syncs) {
        for (const key of sync.keys) {
            const lhs = next[key];
            const rhs = prev[key];

            if (isNullOrUndefined(lhs) && isNullOrUndefined(rhs)) {
                continue;
            }

            if (!Types.equals(lhs, rhs)) {
                return true;
            }
        }
    }

    return false;
}

function isNullOrUndefined(value: any) {
    return Types.isNull(value) || Types.isUndefined(value);
}