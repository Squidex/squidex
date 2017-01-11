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
export class ResolveSchemaGuard implements Resolve<SchemaDetailsDto> {
    constructor(
        private readonly schemasService: SchemasService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<SchemaDetailsDto> {
        const appName = this.findParameter(route, 'appName');
        const schemaName = this.findParameter(route, 'schemaName');

        const result =
            this.schemasService.getSchema(appName, schemaName).toPromise()
                .then(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }

                    return dto;
                }).catch(() => {
                    this.router.navigate(['/404']);
                });

        return result;
    }

    private findParameter(route: ActivatedRouteSnapshot, name: string) {
        let result: string;

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