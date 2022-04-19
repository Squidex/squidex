/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { SchemasState } from '@app/shared/internal';
import { LoadSchemasGuard } from './load-schemas.guard';

describe('LoadSchemasGuard', () => {
    let schemasState: IMock<SchemasState>;
    let schemaGuard: LoadSchemasGuard;

    beforeEach(() => {
        schemasState = Mock.ofType<SchemasState>();
        schemaGuard = new LoadSchemasGuard(schemasState.object);
    });

    it('should load schemas', async () => {
        schemasState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(schemaGuard.canActivate());

        expect(result).toBeTruthy();

        schemasState.verify(x => x.load(), Times.once());
    });
});
