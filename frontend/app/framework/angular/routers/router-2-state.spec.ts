/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NavigationEnd, NavigationExtras, NavigationStart, Params, Router } from '@angular/router';
import { LocalStoreService, MathHelper } from '@app/framework/internal';
import { BehaviorSubject, Subject } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { State } from './../../state';
import { PagingSynchronizer, QueryParams, Router2State, StringKeysSynchronizer, StringSynchronizer } from './router-2-state';

describe('Router2State', () => {
    describe('Strings', () => {
        const synchronizer = new StringSynchronizer('key');

        it('should parse from state', () => {
            const value = 'my-string';

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toEqual({ key: 'my-string' });
        });

        it('should parse from state as undefined when not a string', () => {
            const value = 123;

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toBeUndefined();
        });

        it('should get string from route', () => {
            const params: QueryParams = { key: 'my-string' };

            const value = synchronizer.parseFromRoute(params);

            expect(value).toEqual({ key: 'my-string' });
        });
    });

    describe('StringKeys', () => {
        const synchronizer = new StringKeysSynchronizer('key');

        it('should parse from state', () => {
            const value = { flag1: true, flag2: true };

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toEqual({ key: 'flag1,flag2' });
        });

        it('should parse from state as undefined when empty', () => {
            const value = 123;

            const query = synchronizer.parseFromState({ key: value });

            expect(query).toBeUndefined();
        });

        it('should parse from state as undefined when not an object', () => {
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

        it('should parse from state without page when zero', () => {
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
        let routerQueryParams: BehaviorSubject<Params>;
        let routerEvents: Subject<any>;
        let route: any;
        let router: IMock<Router>;
        let router2State: Router2State;
        let state: State<any>;
        let invoked = 0;

        beforeEach(() => {
            localStore = Mock.ofType<LocalStoreService>();

            routerEvents = new Subject<any>();
            router = Mock.ofType<Router>();
            router.setup(x => x.events).returns(() => routerEvents);

            state = new State<any>({});

            routerQueryParams = new BehaviorSubject<Params>({});
            route = { queryParams: routerQueryParams, id: MathHelper.guid() };

            router2State = new Router2State(route, router.object, localStore.object);
            router2State.mapTo(state)
                .keep('keep')
                .withString('state1')
                .withStrings('state2')
                .whenSynced(() => { invoked++; })
                .build();

            invoked = 0;
        });

        afterEach(() => {
            router2State.ngOnDestroy();
        });

        it('should unsubscribe from route and state', () => {
            router2State.ngOnDestroy();

            expect(state.changes['observers'].length).toEqual(0);
            expect(route.queryParams.observers.length).toEqual(0);
            expect(routerEvents.observers.length).toEqual(0);
        });

        it('Should sync from route', () => {
            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            expect(state.snapshot.state1).toEqual('hello');
            expect(state.snapshot.state2).toEqual({ squidex: true, cms: true });
        });

        it('Should invoke callback after sync from route', () => {
            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            expect(invoked).toEqual(1);
        });

        it('Should not sync again from route when nothing changed', () => {
            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            expect(invoked).toEqual(1);
        });

        it('Should not sync again from route  when changed from null to undefined', () => {
            routerQueryParams.next({
                state1: 'hello',
                state2: null
            });

            routerQueryParams.next({
                state1: 'hello',
                state2: undefined
            });

            routerQueryParams.next({
                state1: 'hello',
                state2: null
            });

            expect(invoked).toEqual(1);
        });

        it('Should not sync again from route  when no state has changed', () => {
            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            routerQueryParams.next({
                state1: 'hello',
                state2: 'cms,squidex'
            });

            expect(invoked).toEqual(1);
        });

        it('Should not sync again from route when other key changed', () => {
            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms',
                state3: 'other'
            });

            expect(invoked).toEqual(1);
        });

        it('Should reset other values when synced from route', () => {
            state.next({ other: 123 });

            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            expect(state.snapshot.other).toBeUndefined();
        });

        it('Should keep configured values when synced from route', () => {
            state.next({ keep: 123 });

            routerQueryParams.next({
                state1: 'hello',
                state2: 'squidex,cms'
            });

            expect(state.snapshot.keep).toBe(123);
        });

        it('Should sync from state', () => {
            let routeExtras: NavigationExtras;

            router.setup(x => x.navigate([], It.isAny()))
                .callback((_, extras) => { routeExtras = extras; });

            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true }
            });

            expect(routeExtras!.replaceUrl).toBeTrue();
            expect(routeExtras!.queryParamsHandling).toBe('merge');
            expect(routeExtras!.queryParams).toEqual({ state1: 'hello', state2: 'squidex,cms' });

            router.verify(x => x.navigate(It.isAny(), It.isAny()), Times.once());
        });

        it('Should sync from state again when nothing has changed', () => {
            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true }
            });

            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true }
            });

            router.verify(x => x.navigate(It.isAny(), It.isAny()), Times.once());

            expect().nothing();
        });

        it('Should not sync from state when navigating', () => {
            routerEvents.next(new NavigationStart(0, ''));

            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true }
            });

            router.verify(x => x.navigate(It.isAny(), It.isAny()), Times.never());

            expect().nothing();
        });

        it('Should sync from state delayed when navigating', () => {
            let routeExtras: NavigationExtras;

            router.setup(x => x.navigate([], It.isAny()))
                .callback((_, extras) => { routeExtras = extras; });

            routerEvents.next(new NavigationStart(0, ''));

            state.next({
                state1: 'hello',
                state2: { squidex: true, cms: true }
            });

            router.verify(x => x.navigate(It.isAny(), It.isAny()), Times.never());

            routerEvents.next(new NavigationEnd(0, '', ''));

            expect(routeExtras!.replaceUrl).toBeTrue();
            expect(routeExtras!.queryParamsHandling).toBe('merge');
            expect(routeExtras!.queryParams).toEqual({ state1: 'hello', state2: 'squidex,cms' });

            router.verify(x => x.navigate(It.isAny(), It.isAny()), Times.once());
        });
    });
});