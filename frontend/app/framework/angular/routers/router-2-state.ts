/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, OnDestroy } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { LocalStoreService, Types } from '@app/framework/internal';
import { State } from '@app/framework/state';
import { Subscription } from 'rxjs';

export type QueryParams = { [name: string]: string };

export interface RouteSynchronizer {
    readonly keys: ReadonlyArray<string>;

    parseFromRoute(query: QueryParams): {} | undefined;

    parseFromState(state: any): QueryParams | undefined;
}

export class PagingSynchronizer implements RouteSynchronizer {
    public readonly keys = ['page', 'pageSize'];

    constructor(
        private readonly localStore: LocalStoreService,
        private readonly storeName: string,
        private readonly defaultSize: number,
    ) {
    }

    public parseFromRoute(query: QueryParams) {
        let pageSize = 0;

        const pageSizeValue = query['pageSize'];

        if (Types.isString(pageSizeValue)) {
            pageSize = parseInt(pageSizeValue, 10);
        }

        if (pageSize <= 0 || pageSize > 100 || !Types.isNumber(pageSize) || Number.isNaN(pageSize)) {
            pageSize = this.localStore.getInt(`${this.storeName}.pageSize`, this.defaultSize);
        }

        if (pageSize <= 0 || pageSize > 100 || !Types.isNumber(pageSize) || Number.isNaN(pageSize)) {
            pageSize = this.defaultSize;
        }

        let page = parseInt(query['page'], 10);

        if (page <= 0 || !Types.isNumber(page) || Number.isNaN(page)) {
            page = 0;
        }

        return { page, pageSize };
    }

    public parseFromState(state: any) {
        let page;

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
        private readonly key: string,
        private readonly fallback?: string,
    ) {
    }

    public parseFromRoute(params: QueryParams) {
        if (!params.hasOwnProperty(this.key)) {
            return { [this.key]: this.fallback };
        }

        const value = params[this.key];

        return { [this.key]: value };
    }

    public parseFromState(state: any) {
        const value = state[this.key];

        if (Types.isString(value)) {
            return { [this.key]: value };
        }

        return undefined;
    }
}

export class StringKeysSynchronizer implements RouteSynchronizer {
    public get keys() {
        return [this.key];
    }

    constructor(
        private readonly key: string,
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

        return undefined;
    }
}

export interface StateSynchronizer {
    mapTo<T extends {}>(state: State<T>): StateSynchronizerMap<T>;
}

export interface StateSynchronizerMap<T> {
    withString(key: keyof T & string): this;

    withStrings(key: keyof T & string): this;

    withPaging(storeName: string, defaultSize: number): this;

    withSynchronizer(synchronizer: RouteSynchronizer): this;

    getInitial(): Partial<T>;
}

@Injectable()
export class Router2State implements OnDestroy, StateSynchronizer {
    private mapper: Router2StateMap<any>;

    constructor(
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly localStore: LocalStoreService,
    ) {
    }

    public getInitial() {
        return this.mapper?.getInitial();
    }

    public listen() {
        this.mapper?.listen();
    }

    public unlisten() {
        this.mapper?.unlisten();
    }

    public ngOnDestroy() {
        this.unlisten();
    }

    public mapTo<T extends {}>(state: State<T>) {
        this.mapper?.unlisten();
        this.mapper = new Router2StateMap<T>(state, this.route, this.router, this.localStore);

        return this.mapper;
    }
}

export class Router2StateMap<T extends {}> implements StateSynchronizerMap<T> {
    private readonly syncs: RouteSynchronizer[] = [];
    private lastSyncedQuery: QueryParams;
    private stateSubscription: Subscription;

    constructor(
        private readonly state: State<T>,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly localStore: LocalStoreService,
    ) {
    }

    public listen() {
        this.stateSubscription = this.state.changes.subscribe(s => this.syncToRoute(s));

        return this;
    }

    public unlisten() {
        this.stateSubscription?.unsubscribe();
    }

    private syncToRoute(state: T) {
        const query: Params = {};

        for (const sync of this.syncs) {
            const values = sync.parseFromState(state);

            for (const key of sync.keys) {
                query[key] = values?.[key];
            }
        }

        if (Types.equals(this.lastSyncedQuery, query)) {
            return;
        }

        this.lastSyncedQuery = query;

        this.router.navigate([], {
            queryParams: query,
            queryParamsHandling: 'merge',
            replaceUrl: true,
        });
    }

    public getInitial() {
        const update: Partial<T> = {};

        const query = this.route.snapshot.queryParams;

        for (const sync of this.syncs) {
            const values = sync.parseFromRoute(query);

            for (const key of sync.keys) {
                update[key] = values?.[key];
            }
        }

        return update;
    }

    public withString(key: keyof T & string) {
        return this.withSynchronizer(new StringSynchronizer(key));
    }

    public withStringOr(key: keyof T & string, fallback: string) {
        return this.withSynchronizer(new StringSynchronizer(key, fallback));
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
}
