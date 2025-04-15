/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiUrlConfig, HTTP, mapVersioned, pretifyError, Resource, Versioned, VersionOrTag } from '@app/framework';
import { AuthSchemeResponseDto, AuthSchemeValueDto, CreateTeamDto, IAuthSchemeValueDto, ICreateTeamDto, IUpdateTeamDto, TeamDto, UpdateTeamDto } from '../model';

@Injectable({
    providedIn: 'root',
})
export class TeamsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getTeams(): Observable<ReadonlyArray<TeamDto>> {
        const url = this.apiUrl.buildUrl('/api/teams');

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return body.map(TeamDto.fromJSON);
            }),
            pretifyError('i18n:teams.loadFailed'));
    }

    public getTeam(teamId: string): Observable<TeamDto> {
        const url = this.apiUrl.buildUrl(`/api/teams/${teamId}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return TeamDto.fromJSON(body);
            }),
            pretifyError('i18n:teams.teamLoadFailed'));
    }

    public postTeam(dto: ICreateTeamDto): Observable<TeamDto> {
        const url = this.apiUrl.buildUrl('api/teams');

        return this.http.post(url, new CreateTeamDto(dto).toJSON()).pipe(
            map(body => {
                return TeamDto.fromJSON(body);
            }),
            pretifyError('i18n:teams.createFailed'));
    }

    public putTeam(teamId: string, resource: Resource, dto: IUpdateTeamDto, version: VersionOrTag): Observable<TeamDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, new UpdateTeamDto(dto).toJSON()).pipe(
            map(({ payload }) => {
                return TeamDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:teams.updateFailed'));
    }

    public getTeamAuth(teamId: string): Observable<Versioned<AuthSchemeResponseDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/auth`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned((payload) => {
                return AuthSchemeResponseDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:teams.teamLoadFailed'));
    }

    public putTeamAuth(teamId: string, dto: IAuthSchemeValueDto, version: VersionOrTag): Observable<Versioned<AuthSchemeResponseDto>> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/auth`);

        return HTTP.putVersioned(this.http, url, new AuthSchemeValueDto(dto).toJSON(), version).pipe(
            mapVersioned((payload) => {
                return AuthSchemeResponseDto.fromJSON(payload.body);
            }),
            pretifyError('i18n:teams.teamLoadFailed'));
    }

    public leaveTeam(teamId: string, resource: Resource): Observable<any> {
        const link = resource._links['leave'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:teams.leaveFailed'));
    }

    public deleteTeam(teamId: string, resource: Resource): Observable<any> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:teams.archiveFailed'));
    }
}