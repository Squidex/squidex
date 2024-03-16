/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NavigationExtras, Params, Router } from '@angular/router';
import { IMock, It, Mock, Times } from 'typemoq';
import { LocalStoreService } from '@app/framework/internal';
import { State } from './../../state';
import { PagingSynchronizer, QueryParams, Router2State, StringKeysSynchronizer, StringSynchronizer } from './router-2-state';

describe('Router2State', () => {
    describe('Strings', () => {
        const synchronizer = new StringSynchronizer('key', 'fallback');

        it('should parse from state', () => {
            const value = 'my-string';

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toEqual({ key: 'my-string' });
        });

        it('should parse from state as undefined if not a string', () => {
            const value = 123;

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toBeUndefined();
        });

        it('should get string from route', () => {
            const params: QueryParams = { key: 'my-string' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ key: 'my-string' });
        });

        it('should not get fallback from route if empty', () => {
            const params: QueryParams = { key: '' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ key: '' });
        });

        it('should get fallback from route if not found', () => {
            const params: QueryParams = { other: 'my-string' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ key: 'fallback' });
        });
    });

    describe('StringKeys', () => {
        const synchronizer = new StringKeysSynchronizer('key');

        it('should parse from state', () => {
            const value = { flag1: true, flag2: true };

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toEqual({ key: 'flag1,flag2' });
        });

        it('should parse from state as undefined if empty', () => {
            const value = 123;

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toBeUndefined();
        });

        it('should parse from state as undefined if not an object', () => {
            const value = 123;

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toBeUndefined();
        });

        it('should get object from route', () => {
            const params: QueryParams = { key: 'flag1,flag2' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ key: { flag1: true, flag2: true } });
        });

        it('should get object with empty keys from route', () => {
            const params: QueryParams = { key: 'flag1,,,flag2' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ key: { flag1: true, flag2: true } });
        });
    });

    describe('Paging', () => {
        let synchronizer: PagingSynchronizer;
        let localStore: IMock<LocalStoreService>;

        beforeEach(() => {
            localStore = Mock.ofType<LocalStoreService>();

            synchronizer = new PagingSynchronizer(localStore.object, 'contents', 30);
        });

        it('should parse from state', () => {
            const state = { page: 10, pageSize: 20 };

            const query = synchronizer.parseFromState(state);

            expect(query).toEqual({ page: '10', pageSize: '20' });

            localStore.verify(x => x.setInt('contents.pageSize', 20), Times.once());
        });

        it('should parse from state without page if zero', () => {
            const state = { page: 0, pageSize: 20 };

            const query = synchronizer.parseFromState(state);

            expect(query).toEqual({ page: undefined, pageSize: '20' });

            localStore.verify(x => x.setInt('contents.pageSize', 20), Times.once());
        });

        it('should get page and size from route', () => {
            const params: Params = { page: '10', pageSize: '40' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ page: 10, pageSize: 40 });
        });

        it('should get page size from local store as fallback', () => {
            localStore.setup(x => x.getInt('contents.pageSize', It.isAny()))
                .returns(() => 40);

            const params: Params = { page: '10' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ page: 10, pageSize: 40 });
        });

        it('should get page size from default if local store is invalid', () => {
            localStore.setup(x => x.getInt('contents.pageSize', It.isAny()))
                .returns(() => -5);

            const params: Params = { page: '10' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ page: 10, pageSize: 30 });
        });

        it('should get page size from default as last fallback', () => {
            const params: Params = { page: '10' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ page: 10, pageSize: 30 });
        });

        it('should fix page number if invalid', () => {
            const params: Params = { page: '-10' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ page: 0, pageSize: 30 });
        });
    });

    describe('Implementation', () => {
        let localStore: IMock<LocalStoreService>;
        let queryParams: QueryParams = {};
        let route: any;
        let router: IMock<Router>;
        let router2State: Router2State;
        let state: State<any>;

        beforeEach(() => {
            localStore = Mock.ofType<LocalStoreService>();

            queryParams = {};

            router = Mock.ofType<Router>();
            route = {
                snapshot: {
                    queryParams,
                },
            };

            state = new State<any>({});

            router2State = new Router2State(route, router.object, localStore.object);
            router2State.mapTo(state)
                .withString('state1')
                .withStrings('state2')
                .listen();
        });

        afterEach(() => {
            router2State.ngOnDestroy();
        });

        it('should unsubscribe from state', () => {
            router2State.ngOnDestroy();

            expect(state.changes['observers'].length).toEqual(0);
        });

        it('Should get values from route', () => {
            queryParams['state1'] = 'hello';
            queryParams['state2'] = 'squidex,cms';

            const values = router2State.getInitial();

            expect(values).toEqual({ state1: 'hello', state2: { squidex: true, cms: true } });
        });

        it('Should sync from state', () => {
            let routeExtras: NavigationExtras;

            router.setup(x => x.navigate([], It.isAny()))
                .callback((_, extras) => { routeExtras = extras; });

            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true },
            });

            expect(routeExtras!.replaceUrl).toBeTrue();
            expect(routeExtras!.queryParamsHandling).toBe('merge');
            expect(routeExtras!.queryParams).toEqual({ state1: 'hello', state2: 'squidex,cms' });

            router.verify(x => x.navigate(It.isAny(), It.isAny()), Times.exactly(2));
        });

        it('Should not sync from state again if nothing has changed', () => {
            let routeExtras: NavigationExtras;

            router.setup(x => x.navigate([], It.isAny()))
                .callback((_, extras) => { routeExtras = extras; });

            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true },
            });

            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true },
            });

            expect(routeExtras!.replaceUrl).toBeTrue();
            expect(routeExtras!.queryParamsHandling).toBe('merge');
            expect(routeExtras!.queryParams).toEqual({ state1: 'hello', state2: 'squidex,cms' });

            router.verify(x => x.navigate(It.isAny(), It.isAny()), Times.exactly(2));
        });
    });
});
