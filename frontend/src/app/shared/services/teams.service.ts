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
import { ApiUrlConfig, DateTime, hasAnyLink, HTTP, pretifyError, Resource, ResourceLinks, Version } from '@app/framework';

export class TeamDto {
    public readonly _links: ResourceLinks;

    public readonly canReadContributors: boolean;
    public readonly canReadPlans: boolean;
    public readonly canUpdateGeneral: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly created: DateTime,
        public readonly createdBy: string,
        public readonly lastModified: DateTime,
        public readonly lastModifiedBy: string,
        public readonly version: Version,
        public readonly name: string,
        public readonly roleName: string,
        public readonly roleProperties: {},
    ) {
        this._links = links;

        this.canReadContributors = hasAnyLink(links, 'contributors');
        this.canReadPlans = hasAnyLink(links, 'plans');
        this.canUpdateGeneral = hasAnyLink(links, 'update');
    }
}

export type CreateTeamDto = Readonly<{
    // The new name of the team. Must not be unique.
    name: string;
}>;

export type UpdateTeamDto = Readonly<{
    // The new name of the team. Must not be unique.
    name?: string;
}>;

@Injectable()
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
                const teams = body.map(parseTeam);

                return teams;
            }),
            pretifyError('i18n:teams.loadFailed'));
    }

    public getTeam(teamName: string): Observable<TeamDto> {
        const url = this.apiUrl.buildUrl(`/api/teams/${teamName}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const team = parseTeam(body);

                return team;
            }),
            pretifyError('i18n:teams.teamLoadFailed'));
    }

    public postTeam(dto: CreateTeamDto): Observable<TeamDto> {
        const url = this.apiUrl.buildUrl('api/teams');

        return this.http.post(url, dto).pipe(
            map(body => {
                return parseTeam(body);
            }),
            pretifyError('i18n:teams.createFailed'));
    }

    public putTeam(teamId: string, resource: Resource, dto: UpdateTeamDto, version: Version): Observable<TeamDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            map(({ payload }) => {
                return parseTeam(payload.body);
            }),
            pretifyError('i18n:teams.updateFailed'));
    }

    public leaveTeam(teamId: string, resource: Resource): Observable<any> {
        const link = resource._links['leave'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:teams.leaveFailed'));
    }
}

function parseTeam(response: any & Resource) {
    return new TeamDto(response._links,
        response.id,
        DateTime.parseISO(response.created), response.createdBy,
        DateTime.parseISO(response.lastModified), response.lastModifiedBy,
        new Version(response.version.toString()),
        response.name,
        response.roleName,
        response.roleProperties);
}