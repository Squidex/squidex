/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { SchemaDto, SchemasState } from '@app/shared/internal';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { SchemaMustExistGuard } from './schema-must-exist.guard';

describe('SchemaMustExistGuard', () => {
    const route: any = {
        params: {
            schemaName: '123',
        },
    };

    let schemasState: IMock<SchemasState>;
    let router: IMock<Router>;
    let schemaGuard: SchemaMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        schemasState = Mock.ofType<SchemasState>();
        schemaGuard = new SchemaMustExistGuard(schemasState.object, router.object);
    });

    it('should load schema and return true if found', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{}));

        let result: boolean;

        schemaGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();
    });

    it('should load schema and return false if not found', () => {
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
