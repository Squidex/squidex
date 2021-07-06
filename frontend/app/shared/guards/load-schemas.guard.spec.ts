/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { SchemasState } from '@app/shared';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { LoadSchemasGuard } from './load-schemas.guard';

describe('LoadSchemasGuard', () => {
    let schemasState: IMock<SchemasState>;
    let schemaGuard: LoadSchemasGuard;

    beforeEach(() => {
        schemasState = Mock.ofType<SchemasState>();
        schemaGuard = new LoadSchemasGuard(schemasState.object);
    });

    it('should load schemas', () => {
        schemasState.setup(x => x.load())
            .returns(() => of(null));

        let result = false;

        schemaGuard.canActivate().subscribe(value => {
            result = value;
        });

        expect(result).toBeTruthy();

        schemasState.verify(x => x.load(), Times.once());
    });
});
