/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Injectable, OnDestroy } from '@angular/core';
import { ActivatedRoute, NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Params, Router } from '@angular/router';
import { LocalStoreService, Pager, Types } from '@app/framework/internal';
import { State } from '@app/framework/state';
import { Subscription } from 'rxjs';

export interface RouteSynchronizer {
    getValue(params: Params): any;

    writeValue(state: any, params: Params): void;
}

export class PagerSynchronizer implements RouteSynchronizer {
    constructor(
        private readonly localStore: LocalStoreService,
        private readonly storeName: string,
        private readonly defaultSize: number
    ) {
    }

    public getValue(params: Params) {
        let pageSize = 0;

        const pageSizeValue = params['take'];

        if (Types.isString(pageSizeValue)) {
            pageSize = parseInt(pageSizeValue, 10);
        }

        if (pageSize <= 0 || pageSize > 100 || isNaN(pageSize)) {
            pageSize = this.localStore.getInt(`${this.storeName}.pageSize`, this.defaultSize);
        }

        if (pageSize <= 0 || pageSize > 100 || isNaN(pageSize)) {
            pageSize = this.defaultSize;
        }

        let page = parseInt(params['page'], 10);

        if (page <= 0 || isNaN(page)) {
            page = 0;
        }

        return new Pager(0, page, pageSize, true);
    }

    public writeValue(state: any, params: Params) {
        params['page'] = undefined;
        params['take'] = undefined;

        if (Types.is(state, Pager)) {
            if (state.page > 0) {
                params['page'] = state.page.toString();
            }

            if (state.pageSize > 0) {
                params['take'] = state.pageSize.toString();

                this.localStore.setInt(`${this.storeName}.pageSize`, state.pageSize);
            }
        }
    }
}

export class StringSynchronizer implements RouteSynchronizer {
    constructor(
        private readonly name: string
    ) {
    }

    public getValue(params: Params) {
        const value = params[this.name];

        return value;
    }

    public writeValue(state: any, params: Params) {
        params[this.name] = undefined;

        if (Types.isString(state)) {
            params[this.name] = state;
        }
    }
}

export class StringKeysSynchronizer implements RouteSynchronizer {
    constructor(
        private readonly name: string
    ) {
    }

    public getValue(params: Params) {
        const value = params[this.name];

        const result: { [key: string]: boolean } = {};

        if (Types.isString(value)) {
            for (const item of value.split(',')) {
                if (item.length > 0) {
                    result[item] = true;
                }
            }
        }

        return result;
    }

    public writeValue(state: any, params: Params) {
        params[this.name] = undefined;

        if (Types.isObject(state)) {
            const value = Object.keys(state).join(',');

            if (value.length > 0) {
                params[this.name] = value;
            }
        }
    }
}

export interface StateSynchronizer {
    mapTo<T extends object>(state: State<T>): StateSynchronizerMap<T>;
}

export interface StateSynchronizerMap<T> {
    keep(key: keyof T & string): this;

    withString(key: keyof T & string, urlName: string): this;

    withStrings(key: keyof T & string, urlName: string): this;

    withPager(key: keyof T & string, storeName: string, defaultSize: number): this;

    whenSynced(action: () => void): this;

    withSynchronizer(key: keyof T & string, synchronizer: RouteSynchronizer): this;

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
    private readonly syncs: { [field: string]: { synchronizer: RouteSynchronizer, value: any } } = {};
    private readonly keysToKeep: string[] = [];
    private syncDone: (() => void)[] = [];
    private lastSyncedParams: Params | undefined;
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
    }

    public destroy() {
        this.syncDone = [];

        this.subscriptionQueryParams?.unsubscribe();
        this.subscriptionChanges?.unsubscribe();
        this.subscriptionEvents?.unsubscribe();
    }

    private syncToRoute(state: T) {
        let isChanged = false;

        for (const key in this.syncs) {
            if (this.syncs.hasOwnProperty(key)) {
                const target = this.syncs[key];

                const value = state[key];

                if (value !== target.value) {
                    target.value = value;

                    isChanged = true;
                }
            }
        }

        if (!isChanged) {
            return;
        }

        const query: Params = {};

        for (const key in this.syncs) {
            if (this.syncs.hasOwnProperty(key)) {
                const { synchronizer, value } = this.syncs[key];

                synchronizer.writeValue(value, query);
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

        this.router.navigate([], {
            queryParams: query,
            queryParamsHandling: 'merge',
            replaceUrl: true
        });

        this.lastSyncedParams = cleanupParams(query);
    }

    private syncFromRoute(query: Params) {
        query = cleanupParams(query);

        if (Types.equals(this.lastSyncedParams, query)) {
            return;
        }

        const update: Partial<T> = {};

        for (const key in this.syncs) {
            if (this.syncs.hasOwnProperty(key)) {
                const target = this.syncs[key];

                const value = target.synchronizer.getValue(query);

                if (value) {
                    update[key] = value;
                }
            }
        }

        for (const key of this.keysToKeep) {
            update[key] = this.state.snapshot[key];
        }

        if (this.state.resetState(update)) {
            for (const action of this.syncDone) {
                action();
            }
        }
    }

    public keep(key: keyof T & string) {
        this.keysToKeep.push(key);

        return this;
    }

    public withString(key: keyof T & string, urlName: string) {
        return this.withSynchronizer(key, new StringSynchronizer(urlName));
    }

    public withStrings(key: keyof T & string, urlName: string) {
        return this.withSynchronizer(key, new StringKeysSynchronizer(urlName));
    }

    public withPager(key: keyof T & string, storeName: string, defaultSize = 10) {
        return this.withSynchronizer(key, new PagerSynchronizer(this.localStore, storeName, defaultSize));
    }

    public whenSynced(action: () => void) {
        this.syncDone.push(action);

        return this;
    }

    public withSynchronizer(key: keyof T & string, synchronizer: RouteSynchronizer) {
        const previous = this.syncs[key];

        this.syncs[key] = { synchronizer, value: previous?.value };

        return this;
    }
}

function cleanupParams(query: Params) {
    for (const key in query) {
        if (query.hasOwnProperty(key)) {
            const value = query[key];

            if (Types.isNull(value) || Types.isUndefined(value)) {
                delete query[key];
            }
        }
    }

    return query;
}