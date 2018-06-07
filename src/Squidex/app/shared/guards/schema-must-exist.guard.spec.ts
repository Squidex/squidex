/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { SchemaDetailsDto } from './../services/schemas.service';
import { SchemasState } from './../state/schemas.state';
import { SchemaMustExistGuard } from './schema-must-exist.guard';

describe('SchemaMustExistGuard', () => {
    const route: any = {
        params: {
            schemaName: '123'
        }
    };

    let schemasState: IMock<SchemasState>;
    let router: IMock<Router>;
    let schemaGuard: SchemaMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        schemasState = Mock.ofType<SchemasState>();
        schemaGuard = new SchemaMustExistGuard(schemasState.object, router.object);
    });

    it('should load schema and return true when found', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDetailsDto>{}));

        let result: boolean;

        schemaGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        schemasState.verify(x => x.select('123'), Times.once());
    });

    it('should load schema and return false when not found', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(null));

        let result: boolean;

        schemaGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});