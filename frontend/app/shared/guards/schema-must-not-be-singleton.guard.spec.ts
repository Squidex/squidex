/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router, RouterStateSnapshot, UrlSegment } from '@angular/router';
import { SchemaDto, SchemasState } from '@app/shared/internal';
import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { SchemaMustNotBeSingletonGuard } from './schema-must-not-be-singleton.guard';

describe('SchemaMustNotBeSingletonGuard', () => {
    const route: any = {
        params: {
            schemaName: '123',
        },
        url: [
            new UrlSegment('schemas', {}),
            new UrlSegment('name', {}),
            new UrlSegment('new', {}),
        ],
    };

    let schemasState: IMock<SchemasState>;
    let router: IMock<Router>;
    let schemaGuard: SchemaMustNotBeSingletonGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        schemasState = Mock.ofType<SchemasState>();
        schemaGuard = new SchemaMustNotBeSingletonGuard(schemasState.object, router.object);
    });

    it('should subscribe to schema and return true if default', () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/name/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Default' }));

        let result: boolean;

        schemaGuard.canActivate(route, state).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    it('should redirect to content if singleton', () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/name/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Singleton' }));

        let result: boolean;

        schemaGuard.canActivate(route, state).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate([state.url, '123']), Times.once());
    });

    it('should redirect to content if singleton on new page', () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/name/new/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Singleton' }));

        let result: boolean;

        schemaGuard.canActivate(route, state).subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeFalsy();

        router.verify(x => x.navigate(['schemas/name/', '123']), Times.once());
    });
});
