/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { map, tap } from 'rxjs/operators';
import { TeamsState } from '../state/teams.state';

export const teamMustExistGuard = (route: ActivatedRouteSnapshot) => {
    const teamsState = inject(TeamsState);
    const teamName = route.params['teamName'];
    const router = inject(Router);

    const result =
        teamsState.select(teamName).pipe(
            tap(team => {
                if (!team) {
                    router.navigate(['/404']);
                }
            }),
            map(team => !!team));

    return result;
};