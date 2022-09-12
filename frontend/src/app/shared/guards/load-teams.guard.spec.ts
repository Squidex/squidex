/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { TeamsState } from '@app/shared/internal';
import { LoadTeamsGuard } from './load-teams.guard';

describe('LoadTeamsGuard', () => {
    let teamsState: IMock<TeamsState>;
    let teamGuard: LoadTeamsGuard;

    beforeEach(() => {
        teamsState = Mock.ofType<TeamsState>();
        teamGuard = new LoadTeamsGuard(teamsState.object);
    });

    it('should load teams', async () => {
        teamsState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(teamGuard.canActivate());

        expect(result).toBeTruthy();

        teamsState.verify(x => x.load(), Times.once());
    });
});
