/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { SchemaDto, SchemasState } from '@app/shared/internal';
import { schemaMustExistPublishedGuard } from './schema-must-exist-published.guard';

describe('SchemaMustExistPublishedGuard', () => {
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

    bit('should load schema and return true if published', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{ isPublished: true }));

        const result = await firstValueFrom(schemaMustExistPublishedGuard(route));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    bit('should load schema and return false if component', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{ isPublished: true, type: 'Component' }));

        const result = await firstValueFrom(schemaMustExistPublishedGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    bit('should load schema and return false if not found', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(null));

        const result = await firstValueFrom(schemaMustExistPublishedGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    bit('should load schema and return false if not published', async () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDto>{ isPublished: false, type: 'Default' }));

        const result = await firstValueFrom(schemaMustExistPublishedGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}
