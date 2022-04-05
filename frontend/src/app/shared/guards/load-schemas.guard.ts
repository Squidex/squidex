/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { SchemasState } from './../state/schemas.state';

@Injectable()
export class LoadSchemasGuard implements CanActivate {
    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.schemasState.load().pipe(map(_ => true));
    }
}
