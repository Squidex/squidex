/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { SchemasState } from '@app/shared/internal';
import { loadSchemasGuard } from './load-schemas.guard';

describe('LoadSchemasGuard', () => {
    let schemasState: IMock<SchemasState>;

    beforeEach(() => {
        schemasState = Mock.ofType<SchemasState>();

        TestBed.configureTestingModule({ providers: [{ provide: SchemasState, useValue: schemasState.object }] });
    });

    bit('should load schemas', async () => {
        schemasState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(loadSchemasGuard());

        expect(result).toBeTruthy();

        schemasState.verify(x => x.load(), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}
