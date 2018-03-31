/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { IMock, Mock, Times } from 'typemoq';
import { Observable } from 'rxjs';

import { SchemasService, SchemaDetailsDto } from '@app/shared';

import { ResolveSchemaGuard } from './resolve-schema.guard';

describe('ResolveSchemaGuard', () => {
    const route: any = {
        params: {
            appName: 'my-app'
        },
        parent: {
            params: {
                schemaName: 'my-schema'
            }
        }
    };

    let router: IMock<Router>;
    let schemasService: IMock<SchemasService>;
    let schemaGuard: ResolveSchemaGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();

        schemasService = Mock.ofType<SchemasService>();
        schemaGuard = new ResolveSchemaGuard(schemasService.object, router.object);
    });

    it('should return schema if loading succeeded', () => {
        const schema: any = {};

        schemasService.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.of(schema));

        let result: SchemaDetailsDto;

        schemaGuard.resolve(route).subscribe(x => {
            result = x!;
        });

        expect(result!).toBe(schema);

        schemasService.verify(x => x.getSchema('my-app', 'my-schema'), Times.once());
    });

    it('should navigate to 404 page if schema is not found', () => {
        schemasService.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.of(null!));

        let result: SchemaDetailsDto;

        schemaGuard.resolve(route).subscribe(x => {
            result = x!;
        });

        expect(result!).toBeNull();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should navigate to 404 page if schema loading fails', () => {
        schemasService.setup(x => x.getSchema('my-app', 'my-schema'))
            .returns(() => Observable.throw({}));

        let result: SchemaDetailsDto;

        schemaGuard.resolve(route).subscribe(x => {
            result = x!;
        });

        expect(result!).toBeNull();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});