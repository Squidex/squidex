/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { SchemasState } from '@app/shared/internal';

export const loadSchemasGuard = () => {
    const schemasState = inject(SchemasState);

    return schemasState.load().pipe(map(_ => true));
};