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
import { loadTeamsGuard } from './load-teams.guard';

describe('LoadTeamsGuard', () => {
    let teamsState: IMock<TeamsState>;

    beforeEach(() => {
        teamsState = Mock.ofType<TeamsState>();

        TestBed.configureTestingModule({ providers: [{ provide: TeamsState, useValue: teamsState.object }] });
    });

    bit('should load teams', async () => {
        teamsState.setup(x => x.load())
            .returns(() => of(null));

        const result = await firstValueFrom(loadTeamsGuard());

        expect(result).toBeTruthy();

        teamsState.verify(x => x.load(), Times.once());
    });
});

function bit(name: string, assertion: (() => PromiseLike<any>) | (() => void)) {
    it(name, () => {
        return TestBed.runInInjectionContext(() => assertion());
    });
}
