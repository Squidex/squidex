/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router, RouterStateSnapshot } from '@angular/router';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import { SchemaDetailsDto } from './../services/schemas.service';
import { SchemasState } from './../state/schemas.state';
import { SchemaMustExistPublishedGuard } from './schema-must-exist-published.guard';

describe('SchemaMustExistPublishedGuard', () => {
    const route: any = {
        params: {
            schemaName: '123'
        }
    };

    let schemasState: IMock<SchemasState>;
    let state: RouterStateSnapshot = <any>{ url: 'current-url' };
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

        schemaGuard.canActivate(route, state).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        schemasState.verify(x => x.select('123'), Times.once());
    });

    it('should load schema and return false when not found', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDetailsDto>{ isPublished: false }));

        let result: boolean;

        schemaGuard.canActivate(route, state).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should load schema and return false when not found', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(null));

        let result: boolean;

        schemaGuard.canActivate(route, state).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should redirect to content when singleton', () => {
        schemasState.setup(x => x.select('123'))
            .returns(() => of(<SchemaDetailsDto>{ isSingleton: true, id: 'schema-id' }));

        let result: boolean;

        schemaGuard.canActivate(route, state).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate([state.url, 'schema-id']), Times.once());
    });
});