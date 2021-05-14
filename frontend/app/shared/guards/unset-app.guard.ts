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
import { AppsState } from './../state/apps.state';

@Injectable()
export class UnsetAppGuard implements CanActivate {
    constructor(
        private readonly appsState: AppsState,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.appsState.select(null).pipe(map(a => a === null));
    }
}
