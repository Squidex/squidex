/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { RoutingCache, SchemasService } from 'shared';
import { RouterMockup } from './router-mockup';
import { ResolveSchemaGuard } from './resolve-schema.guard';

describe('ResolveSchemaGuard', () => {
    const route = {
        params: {
            appName: 'my-app'
        },
        parent: {
            params: {
                schemaName: 'my-schema'
            }
        }
    };

    let schemasService: IMock<SchemasService>;
    let routingCache: IMock<RoutingCache>;

    beforeEach(() => {
        schemasService = Mock.ofType(SchemasService);

        routingCache = Mock.ofType(RoutingCache);
    });

    it('should throw if route does not contain app name', () => {
        const guard = new ResolveSchemaGuard(schemasService.object, <any>new RouterMockup(), routingCache.object);

        expect(() => guard.resolve(<any>{ params: {} }, <any>{})).toThrow('Route must contain app name.');
    });

    it('should throw if route does not contain schema name', () => {
        const guard = new ResolveSchemaGuard(schemasService.object, <any>new RouterMockup(), routingCache.object);

        expect(() => guard.resolve(<any>{ params: { appName: 'my-app' } }, <any>{})).toThrow('Route must contain schema name.');
    });

    it('should provide schema from cache if found', (done) => {
        const schema = { isPublished: true };

        routingCache.setup(x => x.getValue('schema.my-schema'))
            .returns(() => schema);

        const guard = new ResolveSchemaGuard(schemasService.object, <any>new RouterMockup(), routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBe(schema);

                done();
            });
    });

    it('should navigate to 404 page if schema is not found', (done) => {
        schemasService.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.of(null!));
        const router = new RouterMockup();

        const guard = new ResolveSchemaGuard(schemasService.object, <any>router, routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should navigate to 404 page if schema loading fails', (done) => {
        schemasService.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.throw(null!));
        const router = new RouterMockup();

        const guard = new ResolveSchemaGuard(schemasService.object, <any>router, routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBeFalsy();
                expect(router.lastNavigation).toEqual(['/404']);

                done();
            });
    });

    it('should return schema if loading succeeded', (done) => {
        const schema: any = {};

        schemasService.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.of(schema));
        const router = new RouterMockup();

        const guard = new ResolveSchemaGuard(schemasService.object, <any>router, routingCache.object);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBe(schema);

                done();
            });
    });
});