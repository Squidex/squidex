/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock } from 'typemoq';
import { Observable } from 'rxjs';

import { SchemasService } from 'shared';

import { ResolveSchemaGuard } from './resolve-schema.guard';
import { RouterMockup } from './router-mockup';

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

    beforeEach(() => {
        schemasService = Mock.ofType(SchemasService);
    });

    it('should throw if route does not contain app name', () => {
        const guard = new ResolveSchemaGuard(schemasService.object, <any>new RouterMockup());

        expect(() => guard.resolve(<any>{ params: {} }, <any>{})).toThrow('Route must contain app name.');
    });

    it('should throw if route does not contain schema name', () => {
        const guard = new ResolveSchemaGuard(schemasService.object, <any>new RouterMockup());

        expect(() => guard.resolve(<any>{ params: { appName: 'my-app' } }, <any>{})).toThrow('Route must contain schema name.');
    });

    it('should navigate to 404 page if schema is not found', (done) => {
        schemasService.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.of(null!));
        const router = new RouterMockup();

        const guard = new ResolveSchemaGuard(schemasService.object, <any>router);

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

        const guard = new ResolveSchemaGuard(schemasService.object, <any>router);

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

        const guard = new ResolveSchemaGuard(schemasService.object, <any>router);

        guard.resolve(<any>route, <any>{})
            .subscribe(result => {
                expect(result).toBe(schema);

                done();
            });
    });
});