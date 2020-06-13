/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { Injectable, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { LocalStoreService, Pager, Types } from '@app/framework/internal';
import { State } from '@app/framework/state';
import { Subscription } from 'rxjs';

export interface RouteSynchronizer {
    getValue(params: Params): any;

    writeValue(state: any, params: Params): void;
}

class PagerSynchronizer implements RouteSynchronizer {
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
        const pager: Pager = state;

        if (pager.page > 0) {
            params['page'] = pager.page.toString();
        }

        if (pager.pageSize > 0) {
            params['take'] = pager.pageSize.toString();

            this.localStore.setInt(`${this.storeName}.pageSize`, pager.pageSize);
        }
    }
}

class StringSynchronizer implements RouteSynchronizer {
    constructor(
        private readonly name: string
    ) {
    }

    public getValue(params: Params) {
        const value = params[this.name];

        return value;
    }

    public writeValue(state: any, params: Params) {
        if (Types.isString(state)) {
            params[this.name] = state;
        }
    }
}

class StringArraySynchronizer implements RouteSynchronizer {
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
        if (Types.isObject(state)) {
            const value = Object.keys(state).join(',');

            params[this.name] = value;
        }
    }
}

@Injectable()
export class Router2State implements OnDestroy {
    private mapper: Router2StateMap<any>;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly localStore: LocalStoreService
    ) {
    }

    public ngOnDestroy() {
        this.mapper?.ngOnDestroy();
    }

    public map<T extends object>(state: State<T>): Router2StateMap<T> {
        this.mapper?.ngOnDestroy();
        this.mapper = this.mapper || new Router2StateMap<T>(state, this.route, this.router, this.localStore);

        return this.mapper;
    }
}

export class Router2StateMap<T extends object> implements OnDestroy {
    private readonly syncs: { [field: string]: { synchronizer: RouteSynchronizer, value: any } } = {};
    private readonly keysToKeep: string[] = [];
    private syncDone: ((state: any) => void)[] = [];
    private subscriptionChanges: Subscription;
    private subscriptionRouter: Subscription;
    private subscriptionQueryParams: Subscription;
    private noNextSyncFromRoute = false;

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
    }

    public ngOnDestroy() {
        this.syncDone = [];

        this.subscriptionQueryParams?.unsubscribe();
        this.subscriptionChanges?.unsubscribe();
        this.subscriptionRouter?.unsubscribe();
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
                    break;
                }
            }
        }

        if (!isChanged) {
            return;
        }

        const queryParams: Params = {};

        for (const key in this.syncs) {
            if (this.syncs.hasOwnProperty(key)) {
                const { synchronizer, value } = this.syncs[key];

                synchronizer.writeValue(value, queryParams);
            }
        }

        this.noNextSyncFromRoute = true;

        this.router.navigate([], {
            relativeTo: this.route,
            queryParams,
            queryParamsHandling: 'merge',
            replaceUrl: true
        });
    }

    private syncFromRoute(query: Params) {
        if (this.noNextSyncFromRoute) {
            this.noNextSyncFromRoute = false;
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

        this.state.resetState(update);

        for (const action of this.syncDone) {
            action(this.state);
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
        return this.withSynchronizer(key, new StringArraySynchronizer(urlName));
    }

    public withPager(key: keyof T & string, storeName: string, defaultSize = 10) {
        return this.withSynchronizer(key, new PagerSynchronizer(this.localStore, storeName, defaultSize));
    }

    public whenSynced<TState>(action: (state: TState) => void) {
        this.syncDone.push(action);

        return this;
    }

    public withSynchronizer(key: keyof T & string, synchronizer: RouteSynchronizer) {
        const previous = this.syncs[key];

        this.syncs[key] = { synchronizer, value: previous?.value };

        return this;
    }
}