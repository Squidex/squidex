/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';

import { ContentDto, ContentsService } from './../services/contents.service';

@Injectable()
export class ResolveContentGuard implements Resolve<ContentDto> {
    constructor(
        private readonly contentsService: ContentsService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<ContentDto> {
        const appName = this.findParameter(route, 'appName');
        const schemaName = this.findParameter(route, 'schemaName');
        const contentId = this.findParameter(route, 'contentId');

        if (!appName || !schemaName || !contentId) {
            throw 'Route must contain app and schema name and id.';
        }

        const result =
            this.contentsService.getContent(appName, schemaName, contentId).toPromise()
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

    private findParameter(route: ActivatedRouteSnapshot, name: string): string | null {
        let result: string | null = null;

        while (route) {
            result = route.params[name];

            if (result || !route.parent) {
                break;
            }

            route = route.parent;
        }

        return result;
    }
}