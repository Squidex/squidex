/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { TeamsState } from '../state/teams.state';

export const loadTeamsGuard = () => {
    const teamsState = inject(TeamsState);

    return teamsState.load().pipe(map(() => true));
};