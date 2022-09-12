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
import { TeamsState } from './../state/teams.state';

@Injectable()
export class UnsetTeamGuard implements CanActivate {
    constructor(
        private readonly teamsState: TeamsState,
    ) {
    }

    public canActivate(): Observable<boolean> {
        return this.teamsState.select(null).pipe(map(a => a === null));
    }
}
