/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { SchemaDto, SchemasState } from '@app/shared/internal';
import { schemaMustExistGuard } from './schema-must-exist.guard';

describe('SchemaMustExistGuard', () => {
    const route: any = {
        params: {
            schemaName: '123',
        },
    };

    let router: IMock<Router>;
    let schemasState: IMock<SchemasState>;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        schemasState = Mock.ofType<SchemasState>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: Router,
                    useValue: router.object,
                },
                {
                    provide: SchemasState,
                    useValue: schemasState.object,
                },
            ],
        });
    });

    bit('should load schema and return true if found', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{}));

        const result = await firstValueFrom(schemaMustExistGuard(route));

        expect(result).toBeTruthy();
    });

    bit('should load schema and return false if not found', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(null));

        const result = await firstValueFrom(schemaMustExistGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}
