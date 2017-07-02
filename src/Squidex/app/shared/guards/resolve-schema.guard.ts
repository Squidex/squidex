/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';

import { allParameters } from 'framework';

import { SchemaDetailsDto, SchemasService } from './../services/schemas.service';

@Injectable()
export class ResolveSchemaGuard implements Resolve<SchemaDetailsDto> {
    constructor(
        private readonly schemasService: SchemasService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<SchemaDetailsDto> {
        const params = allParameters(route);

        const appName = params['appName'];

        if (!appName) {
            throw 'Route must contain app name.';
        }

        const schemaName = params['schemaName'];

        if (!schemaName) {
            throw 'Route must contain schema name.';
        }

        const result =
            this.schemasService.getSchema(appName, schemaName).toPromise()
                .then(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);

                        return null;
                    }

                    return dto;
                }).catch(() => {
                    this.router.navigate(['/404']);

                    return null;
                });

        return result;
    }
}