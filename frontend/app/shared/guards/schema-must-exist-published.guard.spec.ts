/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { SchemaDetailsDto, SchemasState } from '@app/shared/internal';
import { SchemaMustExistPublishedGuard } from './schema-must-exist-published.guard';

describe('SchemaMustExistPublishedGuard', () => {
    const route: any = {
        params: {
            schemaName: '123'
        }
    };

    let schemasState: IMock<SchemasState>;
    let router: IMock<Router>;
    let schemaGuard: SchemaMustExistPublishedGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        schemasState = Mock.ofType<SchemasState>();
        schemaGuard = new SchemaMustExistPublishedGuard(schemasState.object, router.object);
    });

    it('should load schema and return true when found', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDetailsDto>{ isPublished: true }));

        let result: boolean;

        schemaGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should load schema and return false when not found', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDetailsDto>{ isPublished: false }));

        let result: boolean;

        schemaGuard.canActivate(route).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });
});