/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { allParams } from '@app/framework';

import { SchemasState } from './../state/schemas.state';

@Injectable()
export class SchemaMustExistPublishedGuard implements CanActivate {
    constructor(
        private readonly schemasState: SchemasState,
        private readonly router: Router
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        const schemaName = allParams(route)['schemaName'];

        const result =
            this.schemasState.select(schemaName).pipe(
                tap(dto => {
                    if (!dto || !dto.isPublished) {
                        this.router.navigate(['/404']);
                    }

                    if (dto && dto.isSingleton && state.url.indexOf(dto.id) < 0) {
                        this.router.navigate([state.url, dto.id]);
                    }
                }),
                map(s => s !== null && s.isPublished));

        return result;
    }
}