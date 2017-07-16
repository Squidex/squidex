/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';

import { allParams } from 'framework';

import { SchemaDetailsDto, SchemasService } from './../services/schemas.service';

@Injectable()
export class ResolvePublishedSchemaGuard implements Resolve<SchemaDetailsDto> {
    constructor(
        private readonly schemasService: SchemasService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<SchemaDetailsDto> {
        const params = allParams(route);

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
                    if (!dto || !dto.isPublished) {
                        this.router.navigate(['/404']);

                        return null;
                    }

                    return dto;
                }, error => {
                    this.router.navigate(['/404']);

                    return null;
                });

        return result;
    }
}