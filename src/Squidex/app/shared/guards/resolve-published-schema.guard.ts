/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';

import { SchemaDetailsDto, SchemasService } from './../services/schemas.service';

@Injectable()
export class ResolvePublishedSchemaGuard implements Resolve<SchemaDetailsDto> {
    constructor(
        private readonly schemasService: SchemasService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<SchemaDetailsDto> {
        const appName = this.findParameter(route, 'appName');
        const schemaName = this.findParameter(route, 'schemaName');

        if (!appName || !schemaName) {
            throw 'Route must contain app and schema name.';
        }

        const result =
            this.schemasService.getSchema(appName, schemaName).toPromise()
                .then(dto => {
                    if (!dto || !dto.isPublished) {
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

    private findParameter(route: ActivatedRouteSnapshot, name: string): string | null {
        let result: string | null = null;

        while (route) {
            result = route.params[name];

            if (result) {
                break;
            }

            route = route.parent;
        }

        return result;
    }
}