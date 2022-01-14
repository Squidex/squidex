/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { SchemaDto, SchemasState } from '@app/shared/internal';
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

    it('should load schema and return true if found', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{}));

        const result = await firstValueFrom(schemaGuard.canActivate(route));

        expect(result).toBeTruthy();
    });

    it('should load schema and return false if not found', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(null));

        const result = await firstValueFrom(schemaGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});
