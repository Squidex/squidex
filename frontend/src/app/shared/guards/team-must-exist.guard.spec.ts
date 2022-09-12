/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { TeamsState } from '@app/shared/internal';
import { TeamMustExistGuard } from './team-must-exist.guard';

describe('TeamMustExistGuard', () => {
    const route: any = {
        params: {
            teamName: 'my-team',
        },
    };

    let router: IMock<Router>;
    let teamsState: IMock<TeamsState>;
    let teamGuard: TeamMustExistGuard;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        teamsState = Mock.ofType<TeamsState>();
        teamGuard = new TeamMustExistGuard(teamsState.object, router.object);
    });

    it('should navigate to 404 page if team is not found', async () => {
        teamsState.setup(x => x.select('my-team'))
            .returns(() => of(null));

        const result = await firstValueFrom(teamGuard.canActivate(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    it('should return true if team is found', async () => {
        teamsState.setup(x => x.select('my-team'))
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(teamGuard.canActivate(route));

        expect(result).toBeTruthy();
    });
});
