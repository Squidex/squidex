/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { TeamsState } from './../state/teams.state';

@Injectable()
export class TeamMustExistGuard implements CanActivate {
    constructor(
        private readonly teamsState: TeamsState,
        private readonly router: Router,
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
        const teamName = route.params['teamName'];

        const result =
            this.teamsState.select(teamName).pipe(
                tap(team => {
                    if (!team) {
                        this.router.navigate(['/404']);
                    }
                }),
                map(team => !!team));

        return result;
    }
}
