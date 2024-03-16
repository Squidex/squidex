/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { SchemaDto, SchemasState } from '@app/shared/internal';
import { SchemaMustExistPublishedGuard } from './schema-must-exist-published.guard';

describe('SchemaMustExistPublishedGuard', () => {
    const route: any = {
        params: {
            schemaName: '123',
        },
    };

    let schemasState: IMock<SchemasState>;
    let router: IMock<Router>;
    let schemaGuard: SchemaMustExistPublishedGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        schemasState = Mock.ofType<SchemasState>();
        schemaGuard = new SchemaMustExistPublishedGuard(schemasState.object, router.object);
    });

    it('should load schema and return true if published', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{ isPublished: true }));

        const result = await firstValueFrom(schemaGuard.canActivate(route));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should load schema and return false if component', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{ isPublished: true, type: 'Component' }));

        const result = await firstValueFrom(schemaGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should load schema and return false if not found', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(null));

        const result = await firstValueFrom(schemaGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should load schema and return false if not published', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{ isPublished: false, type: 'Default' }));

        const result = await firstValueFrom(schemaGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});
