/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Router, RouterStateSnapshot, UrlSegment } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { SchemaDto, SchemasState } from '@app/shared/internal';
import { schemaMustNotBeSingletonGuard } from './schema-must-not-be-singleton.guard';

describe('SchemaMustNotBeSingletonGuard', () => {
    const route: any = {
        params: {
            schemaName: '123',
        },
        url: [
            new UrlSegment('schemas', {}),
            new UrlSegment('my-schema', {}),
            new UrlSegment('new', {}),
        ],
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

    bit('should subscribe to schema and return true if default', async () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/my-schema/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Default', properties: {} }));

        const result = await firstValueFrom(schemaMustNotBeSingletonGuard(false)(route, state));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });

    bit('should redirect to content if singleton', async () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/my-schema/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Singleton' }));

        const result = await firstValueFrom(schemaMustNotBeSingletonGuard(false)(route, state));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate([state.url, '123']), Times.once());
    });

    bit('should redirect to content if singleton on new page', async () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/my-schema/new/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Singleton' }));

        const result = await firstValueFrom(schemaMustNotBeSingletonGuard(false)(route, state));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['schemas/my-schema/', '123']), Times.once());
    });

    bit('should redirect to extension if list url is configured', async () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/my-schema/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Default', properties: { contentsListUrl: 'extension' } }));

        const result = await firstValueFrom(schemaMustNotBeSingletonGuard(false)(route, state));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate([state.url, 'extension']), Times.once());
    });

    bit('should redirect to extension if list url is not configured on extension page', async () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/my-schema/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Default', properties: {} }));

        const result = await firstValueFrom(schemaMustNotBeSingletonGuard(true)(route, state));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate([state.url, '..']), Times.once());
    });

    bit('should return true when schemas has extension on extension page', async () => {
        const state: RouterStateSnapshot = <any>{ url: 'schemas/my-schema/' };

        schemasState.setup(x => x.selectedSchema)
            .returns(() => of(<SchemaDto>{ id: '123', type: 'Default', properties: { contentsListUrl: 'extension' } }));

        const result = await firstValueFrom(schemaMustNotBeSingletonGuard(true)(route, state));

        expect(result).toBeTruthy();

        router.verify(x => x.navigate(It.isAny()), Times.never());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}
