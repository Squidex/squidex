/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { DialogService, shareSubscribed, State } from '@app/framework';
import { CreateTeamDto, TeamDto, TeamsService, UpdateTeamDto } from '@app/shared/internal';

interface Snapshot {
    // All teams, loaded once.
    teams: ReadonlyArray<TeamDto>;

    // The selected team.
    selectedTeam: TeamDto | null;
}

@Injectable()
export class TeamsState extends State<Snapshot> {
    public teams =
        this.project(s => s.teams);

    public selectedTeam =
        this.project(s => s.selectedTeam);

    public get teamId() {
        return this.snapshot.selectedTeam?.id || '';
    }

    constructor(
        private readonly teamsService: TeamsService,
        private readonly dialogs: DialogService,
    ) {
        super({
            teams: [],
            selectedTeam: null,
        }, 'Teams');
    }

    public reloadTeams() {
        return this.loadTeam(this.teamId).pipe(
            shareSubscribed(this.dialogs));
    }

    public select(name: string | null): Observable<TeamDto | null> {
        return this.loadTeam(name, true).pipe(
            tap(selectedTeam => {
                this.next({ selectedTeam }, 'Selected');
            }));
    }

    public loadTeam(name: string | null, cached = false) {
        if (!name) {
            return of(null);
        }

        if (cached) {
            const found = this.snapshot.teams.find(x => x.name === name);

            if (found) {
                return of(found);
            }
        }

        return this.teamsService.getTeam(name).pipe(
            tap(team => {
                this.replaceTeam(team);
            }),
            catchError(() => of(null)));
    }

    public load(): Observable<any> {
        return this.teamsService.getTeams().pipe(
            tap(teams => {
                this.next(s => {
                    let selectedTeam = s.selectedTeam;

                    if (selectedTeam) {
                        selectedTeam = teams.find(x => x.id === selectedTeam!.id) || selectedTeam;
                    }

                    return { ...s, teams, selectedTeam };
                }, 'Loading Success');
            }),
            shareSubscribed(this.dialogs));
    }

    public create(request: CreateTeamDto): Observable<TeamDto> {
        return this.teamsService.postTeam(request).pipe(
            tap(created => {
                this.next(s => {
                    const teams = [...s.teams, created].sortByString(x => x.name);

                    return { ...s, teams };
                }, 'Created');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public update(team: TeamDto, request: UpdateTeamDto): Observable<TeamDto> {
        return this.teamsService.putTeam(team.name, team, request, team.version).pipe(
            tap(updated => {
                this.replaceTeam(updated);
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public leave(team: TeamDto): Observable<any> {
        return this.teamsService.leaveTeam(team.name, team).pipe(
            tap(() => {
                this.removeTeam(team);
            }),
            shareSubscribed(this.dialogs));
    }

    private removeTeam(team: TeamDto) {
        this.next(s => {
            const teams = s.teams.filter(x => x.name !== team.name);

            const selectedTeam =
                s.selectedTeam?.id !== team.id ?
                s.selectedTeam :
                null;

            return { ...s, teams, selectedTeam };
        }, 'Deleted');
    }

    private replaceTeam(team: TeamDto) {
        this.next(s => {
            const teams = s.teams.replacedBy('id', team);

            const selectedTeam =
                s.selectedTeam?.id !== team.id ?
                s.selectedTeam :
                team;

            return { ...s, teams, selectedTeam };
        }, 'Updated');
    }
}
