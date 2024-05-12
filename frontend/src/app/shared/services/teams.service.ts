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
import { ApiUrlConfig, DateTime, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, Version, Versioned } from '@app/framework';

export class TeamDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canReadAuth: boolean;
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

        this.canDelete = hasAnyLink(links, 'delete');
        this.canReadAuth = hasAnyLink(links, 'auth');
        this.canReadContributors = hasAnyLink(links, 'contributors');
        this.canReadPlans = hasAnyLink(links, 'plans');
        this.canUpdateGeneral = hasAnyLink(links, 'update');
    }
}

export type TeamAuthSchemeDto = Versioned<TeamAuthSchemePayload>;

export type TeamAuthSchemePayload = {
    // The actual scheme.
    scheme?: AuthSchemeDto | null;

    // True, when the scheme can be updated.
    canUpdate: boolean;
};

export type AuthSchemeDto = Readonly<{
    // The domain name of your user accounts.
    domain: string;

    // The display name for buttons.
    displayName: string;

    // The client ID.
    clientId: string;

    // The client secret.
    clientSecret: string;

    // The authority URL.
    authority: string;

    // The URL to redirect after a signout.
    signoutRedirectUrl?: string;
}>;

export type CreateTeamDto = Readonly<{
    // The new name of the team. Must not be unique.
    name: string;
}>;

export type UpdateTeamDto = Readonly<{
    // The new name of the team. Must not be unique.
    name?: string;
}>;

export type AuthSchemeValueDto = Readonly<{
    // The auth scheme.
    scheme: AuthSchemeDto | null;
}>;

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
                const teams = body.map(parseTeam);

                return teams;
            }),
            pretifyError('i18n:teams.loadFailed'));
    }

    public getTeam(teamId: string): Observable<TeamDto> {
        const url = this.apiUrl.buildUrl(`/api/teams/${teamId}`);

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

    public getTeamAuth(teamId: string): Observable<TeamAuthSchemeDto> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/auth`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned((payload) => {
                return parseTeamAuth(payload.body);
            }),
            pretifyError('i18n:teams.teamLoadFailed'));
    }

    public putTeamAuth(teamId: string, dto: AuthSchemeValueDto, version: Version): Observable<TeamAuthSchemeDto> {
        const url = this.apiUrl.buildUrl(`api/teams/${teamId}/auth`);

        return HTTP.putVersioned(this.http, url, dto, version).pipe(
            mapVersioned((payload) => {
                return parseTeamAuth(payload.body);
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

function parseTeamAuth(response: any & Resource) {
    const { scheme, _links } = response;

    const canUpdate = hasAnyLink(_links, 'update');

    return { scheme, canUpdate };
}