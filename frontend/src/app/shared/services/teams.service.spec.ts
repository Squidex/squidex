/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, DateTime, Resource, ResourceLinks, TeamAuthSchemeDto, TeamDto, TeamsService, Version } from '@app/shared/internal';

describe('TeamsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        TeamsService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get teams',
        inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            let teams: ReadonlyArray<TeamDto>;

            teamsService.getTeams().subscribe(result => {
                teams = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([
                teamResponse(12),
                teamResponse(13),
            ]);

            expect(teams!).toEqual([createTeam(12), createTeam(13)]);
        }));

    it('should make get request to get team',
        inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            let team: TeamDto;

            teamsService.getTeam('my-team').subscribe(result => {
                team = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(teamResponse(12));

            expect(team!).toEqual(createTeam(12));
        }));

    it('should make post request to create team',
        inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            const dto = { name: 'new-team' };

            let team: TeamDto;

            teamsService.postTeam(dto).subscribe(result => {
                team = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(teamResponse(12));

            expect(team!).toEqual(createTeam(12));
        }));

    it('should make put request to update team',
        inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/teams/my-team' },
                },
            };

            let team: TeamDto;

            teamsService.putTeam('my-team', resource, { }, version).subscribe(result => {
                team = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(teamResponse(12));

            expect(team!).toEqual(createTeam(12));
        }));

    it('should make get request to get auth',
        inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            let auth: TeamAuthSchemeDto;

            teamsService.getTeamAuth('my-team').subscribe(result => {
                auth = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/auth');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(teamAuthResponse(12), {
                headers: {
                    etag: '2',
                },
            });

            expect(auth!).toEqual({ payload: { scheme: null, canUpdate: true }, version: new Version('2') });
        }));

    it('should make put request to update auth',
        inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            let auth: TeamAuthSchemeDto;

            teamsService.putTeamAuth('my-team', { scheme: null }, version).subscribe(result => {
                auth = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/auth');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(teamAuthResponse(12), {
                headers: {
                    etag: '2',
                },
            });

            expect(auth!).toEqual({ payload: { scheme: null, canUpdate: true }, version: new Version('2') });
        }));

    it('should make delete request to leave team',
        inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    leave: { method: 'DELETE', href: '/api/teams/my-team/contributors/me' },
                },
            };

            teamsService.leaveTeam('my-team', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/contributors/me');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make delete request to delete team',
    inject([TeamsService, HttpTestingController], (teamsService: TeamsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/teams/my-team' },
                },
            };

            teamsService.deleteTeam('my-team', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/teams/my-team');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    function teamResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            created: buildDate(id, 10),
            createdBy: `creator${id}`,
            lastModified: buildDate(id, 20),
            lastModifiedBy: `modifier${id}`,
            version: key,
            name: `team-name${key}`,
            roleName: `Role${id}`,
            roleProperties: createProperties(id),
            _links: {
                update: { method: 'PUT', href: `teams/${id}` },
            },
        };
    }

    function teamAuthResponse(id: number) {

        return {
            scheme: null,
            _links: {
                update: { method: 'PUT', href: `teams/${id}/auth` },
            },
        };
    }
});

export function createTeam(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `teams/${id}` },
    };

    const key = `${id}${suffix}`;

    return new TeamDto(links,
        `id${id}`,
        DateTime.parseISO(buildDate(id, 10)), `creator${id}`,
        DateTime.parseISO(buildDate(id, 20)), `modifier${id}`,
        new Version(key),
        `team-name${key}`,
        `Role${id}`,
        createProperties(id));
}

function createProperties(id: number) {
    const result = {} as Record<string, any>;

    result[`property${id}`] = true;

    return result;
}

function buildDate(id: number, add = 0) {
    return `${id % 1000 + 2000 + add}-12-11T10:09:08Z`;
}
