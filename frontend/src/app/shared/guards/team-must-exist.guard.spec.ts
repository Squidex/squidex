/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { TeamsState } from '@app/shared/internal';
import { teamMustExistGuard } from './team-must-exist.guard';

describe('TeamMustExistGuard', () => {
    const route: any = {
        params: {
            teamName: 'my-team',
        },
    };

    let router: IMock<Router>;
    let teamsState: IMock<TeamsState>;

    beforeEach(() => {
        router = Mock.ofType<Router>();
        teamsState = Mock.ofType<TeamsState>();

        TestBed.configureTestingModule({
            providers: [
                {
                    provide: Router,
                    useValue: router.object,
                },
                {
                    provide: TeamsState,
                    useValue: teamsState.object,
                },
            ],
        });
    });

    bit('should navigate to 404 page if team is not found', async () => {
        teamsState.setup(x => x.select('my-team'))
            .returns(() => of(null));

        const result = await firstValueFrom(teamMustExistGuard(route));

        expect(result).toBeFalsy();

        router.verify(x => x.navigate(['/404']), Times.once());
    });

    bit('should return true if team is found', async () => {
        teamsState.setup(x => x.select('my-team'))
            .returns(() => of(<any>{}));

        const result = await firstValueFrom(teamMustExistGuard(route));

        expect(result).toBeTruthy();
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}