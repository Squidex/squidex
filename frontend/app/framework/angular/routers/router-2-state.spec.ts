/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NavigationExtras, Params, Router } from '@angular/router';
import { LocalStoreService, MathHelper, Pager } from '@app/framework/internal';
import { BehaviorSubject } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { State } from './../../state';
import { PagerSynchronizer, Router2State, StringKeysSynchronizer, StringSynchronizer } from './router-2-state';

describe('Router2State', () => {
    describe('Strings', () => {
        const synchronizer = new StringSynchronizer('key');

        it('should write string to route', () => {
            const params: Params = {};

            const value = 'my-string';

            synchronizer.writeValue(value, params);

            expect(params['key']).toEqual('my-string');
        });

        it('should not write value to route when not a string', () => {
            const params: Params = {};

            const value = 123;

            synchronizer.writeValue(value, params);

            expect(params).toEqual({});
        });

        it('should get string from route', () => {
            const params: Params = {
                key: 'my-string'
            };

            const value = synchronizer.getValue(params);

            expect(value).toEqual('my-string');
        });
    });

    describe('StringKeys', () => {
        const synchronizer = new StringKeysSynchronizer('key');

        it('should write object keys to route', () => {
            const params: Params = {};

            const value = {
                flag1: true,
                flag2: false
            };

            synchronizer.writeValue(value, params);

            expect(params['key']).toEqual('flag1,flag2');
        });

        it('should write empty object to route', () => {
            const params: Params = {};

            const value = {};

            synchronizer.writeValue(value, params);

            expect(params['key']).toEqual('');
        });

        it('should not write value to route when not an object', () => {
            const params: Params = {};

            const value = 123;

            synchronizer.writeValue(value, params);

            expect(params).toEqual({});
        });

        it('should get object from route', () => {
            const params: Params = { key: 'flag1,flag2' };

            const value = synchronizer.getValue(params);

            expect(value).toEqual({ flag1: true, flag2: true });
        });

        it('should get object with empty keys from route', () => {
            const params: Params = { key: 'flag1,,,flag2' };

            const value = synchronizer.getValue(params);

            expect(value).toEqual({ flag1: true, flag2: true });
        });
    });

    describe('Pager', () => {
        let synchronizer: PagerSynchronizer;
        let localStore: IMock<LocalStoreService>;

        beforeEach(() => {
            localStore = Mock.ofType<LocalStoreService>();

            synchronizer = new PagerSynchronizer(localStore.object, 'contents', 30);
        });

        it('should write pager to route and local store', () => {
            const params: Params = {};

            const value = new Pager(0, 10, 20, true);

            synchronizer.writeValue(value, params);

            expect(params['page']).toEqual('10');
            expect(params['take']).toEqual('20');

            localStore.verify(x => x.setInt('contents.pageSize', 20), Times.once());
        });

        it('should not write page if zero', () => {
            const params: Params = {};

            const value = new Pager(0, 0, 20, true);

            synchronizer.writeValue(value, params);

            expect(params['page']).toBeUndefined();
            expect(params['take']).toEqual('20');

            localStore.verify(x => x.setInt('contents.pageSize', 20), Times.once());
        });

        it('should not write value to route when not pager', () => {
            const params: Params = {};

            const value = 123;

            synchronizer.writeValue(value, params);

            expect(params).toEqual({});

            localStore.verify(x => x.setInt('contents.pageSize', 20), Times.never());
        });

        it('should not write value to route when null', () => {
            const params: Params = {};

            const value = null;

            synchronizer.writeValue(value, params);

            expect(params).toEqual({});
            localStore.verify(x => x.setInt('contents.pageSize', 20), Times.never());
        });

        it('should get page and size from route', () => {
            const params: Params = { page: '10', take: '40' };

            const value = synchronizer.getValue(params);

            expect(value).toEqual(new Pager(0, 10, 40, true));
        });

        it('should get page size from local store as fallback', () => {
            localStore.setup(x => x.getInt('contents.pageSize', It.isAny()))
                .returns(() => 40);

            const params: Params = { page: '10' };

            const value = synchronizer.getValue(params);

            expect(value).toEqual(new Pager(0, 10, 40, true));
        });

        it('should get page size from default if local store is invalid', () => {
            localStore.setup(x => x.getInt('contents.pageSize', It.isAny()))
                .returns(() => -5);

            const params: Params = { page: '10' };

            const value = synchronizer.getValue(params);

            expect(value).toEqual(new Pager(0, 10, 30, true));
        });

        it('should get page size from default as last fallback', () => {
            const params: Params = { page: '10' };

            const value = synchronizer.getValue(params);

            expect(value).toEqual(new Pager(0, 10, 30, true));
        });

        it('should fix page number if invalid', () => {
            const params: Params = { page: '-10' };

            const value = synchronizer.getValue(params);

            expect(value).toEqual(new Pager(0, 0, 30, true));
        });
    });

    describe('Implementation', () => {
        let localStore: IMock<LocalStoreService>;
        let routerQueryParams: BehaviorSubject<Params>;
        let routeActivated: any;
        let router: IMock<Router>;
        let router2State: Router2State;
        let state: State<any>;
        let invoked = 0;

        beforeEach(() => {
            localStore = Mock.ofType<LocalStoreService>();

            router = Mock.ofType<Router>();

            state = new State<any>({});

            routerQueryParams = new BehaviorSubject<Params>({});
            routeActivated = { queryParams: routerQueryParams, id: MathHelper.guid() };
            router2State = new Router2State(routeActivated, router.object, localStore.object);

            router2State.mapTo(state)
                .keep('keep')
                .withString('state1', 'key1')
                .withString('state2', 'key2')
                .whenSynced(() => { invoked++; })
                .build();

            invoked = 0;
        });

        afterEach(() => {
            router2State.ngOnDestroy();
        });

        it('should unsubscribe from route and state', () => {
            router2State.ngOnDestroy();

            expect(state.changes['observers'].length).toBe(0);
            expect(routeActivated.queryParams.observers.length).toBe(0);
        });

        it('Should sync from route', () => {
            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex'
            });

            expect(state.snapshot.state1).toEqual('hello');
            expect(state.snapshot.state2).toEqual('squidex');
        });

        it('Should invoke callback after sync from route', () => {
            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex'
            });

            expect(invoked).toEqual(1);
        });

        it('Should not sync again when nothing changed', () => {
            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex'
            });

            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex'
            });

            expect(invoked).toEqual(1);
        });

        it('Should sync again when new query changed', () => {
            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex'
            });

            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex',
                key3: '!'
            });

            expect(invoked).toEqual(2);
        });

        it('Should reset other values when synced from route', () => {
            state.next({ other: 123 });

            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex'
            });

            expect(state.snapshot.other).toBeUndefined();
        });

        it('Should keep configued values when synced from route', () => {
            state.next({ keep: 123 });

            routerQueryParams.next({
                key1: 'hello',
                key2: 'squidex'
            });

            expect(state.snapshot.keep).toBe(123);
        });

        it('Should sync from state', () => {
            let routeExtras: NavigationExtras;

            router.setup(x => x.navigate([], It.isAny()))
                .callback((_, extras) => { routeExtras = extras; });

            state.next({
                state1: 'hello',
                state2: 'squidex'
            });

            expect(routeExtras!.relativeTo).toBeDefined();
            expect(routeExtras!.replaceUrl).toBeTrue();
            expect(routeExtras!.queryParamsHandling).toBe('merge');
            expect(routeExtras!.queryParams).toEqual({ key1: 'hello', key2: 'squidex' });
        });
    });
});