/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { SchemasState } from '@app/shared/internal';

@Injectable()
export class LoadSchemasGuard  {
    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.schemasState.load().pipe(map(_ => true));
    }
}
