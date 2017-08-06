/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams, RoutingCache } from 'framework';

import { SchemaDetailsDto, SchemasService } from './../services/schemas.service';

@Injectable()
export class ResolvePublishedSchemaGuard implements Resolve<SchemaDetailsDto> {
    constructor(
        private readonly schemasService: SchemasService,
        private readonly router: Router,
        private readonly routingCache: RoutingCache
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<SchemaDetailsDto> {
        const params = allParams(route);

        const appName = params['appName'];

        if (!appName) {
            throw 'Route must contain app name.';
        }

        const schemaName = params['schemaName'];

        if (!schemaName) {
            throw 'Route must contain schema name.';
        }

        const schema = this.routingCache.getValue<SchemaDetailsDto>(`schema.${schemaName}`);

        if (schema && schema.isPublished) {
            return Observable.of(schema);
        }

        const result =
            this.schemasService.getSchema(appName, schemaName)
                .do(dto => {
                    if (!dto || !dto.isPublished) {
                        this.router.navigate(['/404']);
                    }
                })
                .catch(error => {
                    this.router.navigate(['/404']);

                    return Observable.of(error);
                });

        return result;
    }
}