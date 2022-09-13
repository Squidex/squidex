/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { TeamsState } from '@app/shared/internal';
import { UnsetTeamGuard } from './unset-team.guard';

describe('UnsetTeamGuard', () => {
    let teamsState: IMock<TeamsState>;
    let teamGuard: UnsetTeamGuard;

    beforeEach(() => {
        teamsState = Mock.ofType<TeamsState>();
        teamGuard = new UnsetTeamGuard(teamsState.object);
    });

    it('should unselect team', async () => {
        teamsState.setup(x => x.select(null))
            .returns(() => of(null));

        const result = await firstValueFrom(teamGuard.canActivate());

        expect(result).toBeTruthy();

        teamsState.verify(x => x.select(null), Times.once());
    });
});
