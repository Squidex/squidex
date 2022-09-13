/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, DateTime, Resource, ResourceLinks, TeamDto, TeamsService, Version } from '@app/shared/internal';

describe('TeamsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
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

    function teamResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10:00Z`,
            createdBy: `creator${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10:00Z`,
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
});

export function createTeam(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `teams/${id}` },
    };

    const key = `${id}${suffix}`;

    return new TeamDto(links,
        `id${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-12-12T10:10:00Z`), `creator${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`), `modifier${id}`,
        new Version(key),
        `team-name${key}`,
        `Role${id}`,
        createProperties(id));
}

function createProperties(id: number) {
    const result = {};

    result[`property${id}`] = true;

    return result;
}
