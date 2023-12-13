/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { TeamsState } from '@app/shared/internal';
import { unsetTeamGuard } from './unset-team.guard';

describe('UnsetTeamGuard', () => {
    let teamsState: IMock<TeamsState>;

    beforeEach(() => {
        teamsState = Mock.ofType<TeamsState>();

        TestBed.configureTestingModule({ providers: [{ provide: TeamsState, useValue: teamsState.object }] });
    });

    bit('should unselect team', async () => {
        teamsState.setup(x => x.select(null))
            .returns(() => of(null));

        const result = await firstValueFrom(unsetTeamGuard());

        expect(result).toBeTruthy();

        teamsState.verify(x => x.select(null), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}