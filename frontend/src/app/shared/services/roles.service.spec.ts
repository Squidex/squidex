/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AnalyticsService, ApiUrlConfig, Resource, ResourceLinks, RoleDto, RolesDto, RolesPayload, RolesService, Version } from '@app/shared/internal';

describe('RolesService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                RolesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get all permissions',
        inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
            let permissions: ReadonlyArray<string>;

            roleService.getPermissions('my-app').subscribe(result => {
                permissions = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/roles/permissions');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(['P1', 'P2']);

            expect(permissions!).toEqual(['P1', 'P2']);
        }));

    it('should make get request to get roles',
        inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
            let roles: RolesDto;

            roleService.getRoles('my-app').subscribe(result => {
                roles = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/roles');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(rolesResponse(2, 4), {
                headers: {
                    etag: '2',
                },
            });

            expect(roles!).toEqual({ payload: createRoles(2, 4), version: new Version('2') });
        }));

    it('should make post request to add role',
        inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
            const dto = { name: 'Role3' };

            let roles: RolesDto;

            roleService.postRole('my-app', dto, version).subscribe(result => {
                roles = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/roles');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(rolesResponse(2, 4), {
                headers: {
                    etag: '2',
                },
            });

            expect(roles!).toEqual({ payload: createRoles(2, 4), version: new Version('2') });
        }));

    it('should make put request to update role',
        inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
            const dto = { permissions: ['P4', 'P5'], properties: createProperties(1) };

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/roles/role1' },
                },
            };

            let roles: RolesDto;

            roleService.putRole('my-app', resource, dto, version).subscribe(result => {
                roles = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/roles/role1');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(rolesResponse(2, 4), {
                headers: {
                    etag: '2',
                },
            });

            expect(roles!).toEqual({ payload: createRoles(2, 4), version: new Version('2') });
        }));

    it('should make delete request to remove role',
        inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/roles/role1' },
                },
            };

            let roles: RolesDto;

            roleService.deleteRole('my-app', resource, version).subscribe(result => {
                roles = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/roles/role1');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush(rolesResponse(2, 4), {
                headers: {
                    etag: '2',
                },
            });

            expect(roles!).toEqual({ payload: createRoles(2, 4), version: new Version('2') });
        }));

    function rolesResponse(...ids: number[]) {
        return {
            items: ids.map(id => ({
                name: `name${id}`,
                numClients: id * 2,
                numContributors: id * 3,
                permissions: createPermissions(id),
                properties: createProperties(id),
                isDefaultRole: id % 2 === 0,
                _links: {
                    update: { method: 'PUT', href: `/roles/id${id}` },
                },
            })),
            _links: {
                create: { method: 'POST', href: '/roles' },
            },
        };
    }
});

export function createRoles(...ids: ReadonlyArray<number>): RolesPayload {
    return {
        items: ids.map(createRole),
        _links: {
            create: { method: 'POST', href: '/roles' },
        },
        canCreate: true,
    };
}

export function createRole(id: number) {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/roles/id${id}` },
    };

    return new RoleDto(links, `name${id}`, id * 2, id * 3,
        createPermissions(id),
        createProperties(id),
        id % 2 === 0);
}

function createPermissions(id: number) {
    const result: string[] = [];

    result.push(`permission${id}`);

    return result;
}

function createProperties(id: number) {
    const result = {};

    result[`property${id}`] = true;

    return result;
}
