/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { ContentsService, RoutingCache } from 'shared';

import { ResolveContentGuard } from './resolve-content.guard';
import { RouterMockup } from './router-mockup';

describe('ResolveContentGuard', () => {
    const route = {
        params: {
            appName: 'my-app'
        },
        parent: {
            params: {
                schemaName: 'my-schema'
            },
            parent: {
                params: {
                    contentId: '123'
                }
            }
        }
    };

    let appsStore: IMock<ContentsService>;
    let routingCache: IMock<RoutingCache>;

    beforeEach(() => {
        appsStore = Mock.ofType(ContentsService);

        routingCache = Mock.ofType(RoutingCache);
    });

    it('should throw if route does not contain app name', () => {
        const guard = new ResolveContentGuard(appsStore.object, <any>new RouterMockup(), routingCache.object);

        expect(() => guard.resolve(<any>{ params: {} }, <any>{})).toThrow('Route must contain app name.');
    });

    it('should throw if route does not contain schema name', () => {
        const guard = new ResolveContentGuard(appsStore.object, <any>new RouterMockup(), routingCache.object);

        expect(() => guard.resolve(<any>{ params: { appName: 'my-app' } }, <any>{})).toThrow('Route must contain schema name.');
    });

    it('should throw if route does not contain content id', () => {
        const guard = new ResolveContentGuard(appsStore.object, <any>new RouterMockup(), routingCache.object);

        expect(() => guard.resolve(<any>{ params: { appName: 'my-app', schemaName: 'my-schema' } }, <any>{})).toThrow('Route must contain content id.');
    });

    it('should provide content from cache if found', (done) => {
        const content = { };

        routingCache.setup(x => x.getValue('content.123'))
            .returns(() => content);

        const guard = new ResolveContentGuard(appsStore.object, <any>new RouterMockup(), routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBe(content);

                done();
            });
    });

    it('should navigate to 404 page if schema is not found', (done) => {
        appsStore.setup(x => x.getContent('my-app', 'my-schema', '123'))
            .returns(() => Observable.of(null!));
        const router = new RouterMockup();

        const guard = new ResolveContentGuard(appsStore.object, <any>router, routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should navigate to 404 page if schema loading fails', (done) => {
        appsStore.setup(x => x.getContent('my-app', 'my-schema', '123'))
            .returns(() => Observable.throw(null!));
        const router = new RouterMockup();

        const guard = new ResolveContentGuard(appsStore.object, <any>router, routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return content if loading succeeded', (done) => {
        const content: any = {};

        appsStore.setup(x => x.getContent('my-app', 'my-schema', '123'))
            .returns(() => Observable.of(content));
        const router = new RouterMockup();

        const guard = new ResolveContentGuard(appsStore.object, <any>router, routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBe(content);

                done();
            });
    });
});