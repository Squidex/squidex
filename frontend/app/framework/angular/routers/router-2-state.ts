/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Injectable, OnDestroy } from '@angular/core';
import { ActivatedRoute, NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Params, Router } from '@angular/router';
import { LocalStoreService, Types } from '@app/framework/internal';
import { State } from '@app/framework/state';
import { Subscription } from 'rxjs';

export interface RouteSynchronizer {
    parseValuesFromRoute(params: Params): object;

    writeValuesToRoute(state: any, params: Params): void;
}

export class PagingSynchronizer implements RouteSynchronizer {
    constructor(
        private readonly localStore: LocalStoreService,
        private readonly storeName: string,
        private readonly defaultSize: number
    ) {
    }

    public parseValuesFromRoute(params: Params) {
        let pageSize = 0;

        const pageSizeValue = params['pageSize'];

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

        return { page, pageSize };
    }

    public writeValuesToRoute(state: any, params: Params) {
        const page: number = state.page;

        if (page > 0) {
            params['page'] = page.toString();
        } else {
            params['page'] = undefined;
        }

        const pageSize: number = state.pageSize;

        params['pageSize'] = pageSize.toString();

        this.localStore.setInt(`${this.storeName}.pageSize`, pageSize);
    }
}

export class StringSynchronizer implements RouteSynchronizer {
    constructor(
        private readonly nameState: string,
        private readonly nameUrl: string
    ) {
    }

    public parseValuesFromRoute(params: Params) {
        const value = params[this.nameUrl];

        return { [this.nameState]: value };
    }

    public writeValuesToRoute(state: any, params: Params) {
        params[this.nameUrl] = undefined;

        const value = state[this.nameState];

        if (Types.isString(value)) {
            params[this.nameUrl] = value;
        }
    }
}

export class StringKeysSynchronizer implements RouteSynchronizer {
    constructor(
        private readonly nameState: string,
        private readonly nameUrl: string
    ) {
    }

    public parseValuesFromRoute(params: Params) {
        const value = params[this.nameUrl];

        const result: { [key: string]: boolean } = {};

        if (Types.isString(value)) {
            for (const item of value.split(',')) {
                if (item.length > 0) {
                    result[item] = true;
                }
            }
        }

        return { [this.nameState]: result };
    }

    public writeValuesToRoute(state: any, params: Params) {
        params[this.nameUrl] = undefined;

        const value = state[this.nameState];

        if (Types.isObject(value)) {
            const items = Object.keys(value).join(',');

            if (items.length > 0) {
                params[this.nameUrl] = items;
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
    private readonly syncs: { synchronizer: RouteSynchronizer, value: object }[] = [];
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

        for (const target of this.syncs) {
            if (!target.value) {
                isChanged = true;
            }

            for (const key in target.value) {
                if (target.value[key] !== state[key]) {
                    isChanged = true;
                    break;
                }
            }

            if (isChanged) {
                break;
            }
        }

        if (!isChanged) {
            return;
        }

        const query: Params = {};

        for (const target of this.syncs) {
            target.synchronizer.writeValuesToRoute(state, query);
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

        for (const target of this.syncs) {
            const values = target.synchronizer.parseValuesFromRoute(query);

            for (const key in values) {
                if (values.hasOwnProperty(key)) {
                    update[key] = values[key];
                }
            }

            target.value = values;
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
        return this.withSynchronizer(new StringSynchronizer(key, urlName));
    }

    public withStrings(key: keyof T & string, urlName: string) {
        return this.withSynchronizer(new StringKeysSynchronizer(key, urlName));
    }

    public withPaging(storeName: string, defaultSize = 10) {
        return this.withSynchronizer(new PagingSynchronizer(this.localStore, storeName, defaultSize));
    }

    public withSynchronizer(synchronizer: RouteSynchronizer) {
        this.syncs.push({ synchronizer, value: {} });

        return this;
    }

    public whenSynced(action: () => void) {
        this.syncDone.push(action);

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