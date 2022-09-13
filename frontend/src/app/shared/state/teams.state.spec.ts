/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of, throwError } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { DialogService, TeamsService, TeamsState } from '@app/shared/internal';
import { createTeam } from './../services/teams.service.spec';

describe('TeamsState', () => {
    const team1 = createTeam(1);
    const team2 = createTeam(2);

    let dialogs: IMock<DialogService>;
    let teamsService: IMock<TeamsService>;
    let teamsState: TeamsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        teamsService = Mock.ofType<TeamsService>();

        teamsService.setup(x => x.getTeams())
            .returns(() => of([team1, team2])).verifiable(Times.atLeastOnce());

        teamsState = new TeamsState(teamsService.object, dialogs.object);
        teamsState.load().subscribe();
    });

    afterEach(() => {
        teamsService.verifyAll();
    });

    it('should load teams', () => {
        expect(teamsState.snapshot.teams).toEqual([team1, team2]);
    });

    it('should select team', async () => {
        const teamSelect = await firstValueFrom(teamsState.select(team1.name));

        expect(teamSelect).toBe(team1);
        expect(teamsState.snapshot.selectedTeam).toBe(team1);
    });

    it('should return null on select if unselecting team', async () => {
        const teamSelected = await firstValueFrom(teamsState.select(null));

        expect(teamSelected).toBeNull();
        expect(teamsState.snapshot.selectedTeam).toBeNull();
    });

    it('should return null on select if team is not found', async () => {
        teamsService.setup(x => x.getTeam('unknown'))
            .returns(() => throwError(() => 'Service Error'));

        const teamSelected = await firstValueFrom(teamsState.select('unknown'));

        expect(teamSelected).toBeNull();
        expect(teamsState.snapshot.selectedTeam).toBeNull();
    });

    it('should return new team if loaded', async () => {
        const newTeam = createTeam(1, '_new');

        teamsService.setup(x => x.getTeam(team1.name))
            .returns(() => of(newTeam));

        const teamSelected = await firstValueFrom(teamsState.loadTeam(team1.name));

        expect(teamSelected).toEqual(newTeam);
        expect(teamsState.snapshot.selectedTeam).toBeNull();
    });

    it('should add team to snapshot if created', () => {
        const updated = createTeam(3, '_new');

        const request = { ...updated };

        teamsService.setup(x => x.postTeam(request))
            .returns(() => of(updated)).verifiable();

        teamsState.create(request).subscribe();

        expect(teamsState.snapshot.teams).toEqual([team1, team2, updated]);
    });

    it('should update team if updated', () => {
        const request = {};

        const updated = createTeam(2, '_new');

        teamsService.setup(x => x.putTeam(team2.name, team2, request, team2.version))
            .returns(() => of(updated)).verifiable();

        teamsState.update(team2, request).subscribe();

        expect(teamsState.snapshot.teams).toEqual([team1, updated]);
    });

    it('should remove team from snapshot if left', () => {
        teamsService.setup(x => x.leaveTeam(team2.name, team2))
            .returns(() => of({})).verifiable();

        teamsState.leave(team2).subscribe();

        expect(teamsState.snapshot.teams).toEqual([team1]);
    });

    describe('Selection', () => {
        beforeEach(() => {
            teamsState.select(team1.name).subscribe();
        });

        it('should update selected team if reloaded', () => {
            const newTeams = [
                createTeam(1, '_new'),
                createTeam(2, '_new'),
            ];

            teamsService.setup(x => x.getTeams())
                .returns(() => of(newTeams));

            teamsState.load().subscribe();

            expect(teamsState.snapshot.selectedTeam).toEqual(newTeams[0]);
        });

        it('should update selected team if updated', () => {
            const request = {};

            const updated = createTeam(1, '_new');

            teamsService.setup(x => x.putTeam(team1.name, team1, request, team1.version))
                .returns(() => of(updated)).verifiable();

            teamsState.update(team1, request).subscribe();

            expect(teamsState.snapshot.selectedTeam).toEqual(updated);
        });
    });
});
