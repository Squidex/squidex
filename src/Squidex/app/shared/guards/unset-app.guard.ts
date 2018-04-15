/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';
import { Observable } from 'rxjs';

import { AppsState } from './../state/apps.state';

@Injectable()
export class UnsetAppGuard implements CanActivate {
    constructor(
        private readonly appsState: AppsState
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.appsState.select(null).map(a => a === null);
    }
}