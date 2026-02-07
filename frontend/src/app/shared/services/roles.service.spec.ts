/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, Resource, RoleDto, RolesDto, RolesService, Versioned, VersionTag } from '@app/shared/internal';
import { AddRoleDto, ResourceLinkDto, UpdateRoleDto } from './../model';

describe('RolesService', () => {
    const version = new VersionTag('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [],
            providers: [
                RolesService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                provideHttpClient(withInterceptorsFromDi()),
                provideHttpClientTesting(),
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get all permissions', inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
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

    it('should make get request to get roles', inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
        let roles: Versioned<RolesDto>;
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

        expect(roles!).toEqual({ payload: createRoles(2, 4), version: new VersionTag('2') });
    }));

    it('should make post request to add role', inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
        const dto = new AddRoleDto({ name: 'Role3' });

        let roles: Versioned<RolesDto>;
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

        expect(roles!).toEqual({ payload: createRoles(2, 4), version: new VersionTag('2') });
    }));

    it('should make put request to update role', inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
        const dto = new UpdateRoleDto({ permissions: [], properties: createProperties(1) });

        const resource: Resource = {
            _links: {
                update: { method: 'PUT', href: '/api/apps/my-app/roles/role1' },
            },
        };

        let roles: Versioned<RolesDto>;
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

        expect(roles!).toEqual({ payload: createRoles(2, 4), version: new VersionTag('2') });
    }));

    it('should make delete request to remove role', inject([RolesService, HttpTestingController], (roleService: RolesService, httpMock: HttpTestingController) => {
        const resource: Resource = {
            _links: {
                delete: { method: 'DELETE', href: '/api/apps/my-app/roles/role1' },
            },
        };

        let roles: Versioned<RolesDto>;
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

        expect(roles!).toEqual({ payload: createRoles(2, 4), version: new VersionTag('2') });
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

export function createRoles(...ids: ReadonlyArray<number>) {
    return new RolesDto({
        items: ids.map(createRole),
        _links: {
            create: new ResourceLinkDto({ method: 'POST', href: '/roles' }),
        },
    });
}

export function createRole(id: number) {
    return new RoleDto({
        name: `name${id}`,
        numClients: id * 2,
        numContributors: id * 3,
        permissions: createPermissions(id),
        properties: createProperties(id),
        isDefaultRole: id % 2 === 0,
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/roles/id${id}` }),
        },
    });
}

function createPermissions(id: number) {
    const result: string[] = [];
    result.push(`permission${id}`);
    return result;
}

function createProperties(id: number) {
    const result = {} as Record<string, any>;
    result[`property${id}`] = true;
    return result;
}
