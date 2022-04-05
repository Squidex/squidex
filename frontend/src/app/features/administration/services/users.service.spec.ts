/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, Resource, ResourceLinks } from '@app/framework';
import { UserDto, UsersDto, UsersService } from './users.service';

describe('UsersService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                UsersService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get many users',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            let users: UsersDto;

            userManagementService.getUsers(20, 30).subscribe(result => {
                users = result;
            });

            const req = httpMock.expectOne('http://service/p/api/user-management?take=20&skip=30&query=');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                total: 100,
                items: [
                    userResponse(12),
                    userResponse(13),
                ],
            });

            expect(users!).toEqual(
                new UsersDto(100, [
                    createUser(12),
                    createUser(13),
                ]));
        }));

    it('should make get request with query to get many users',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            let users: UsersDto;

            userManagementService.getUsers(20, 30, 'my-query').subscribe(result => {
                users = result;
            });

            const req = httpMock.expectOne('http://service/p/api/user-management?take=20&skip=30&query=my-query');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                total: 100,
                items: [
                    userResponse(12),
                    userResponse(13),
                ],
            });

            expect(users!).toEqual(
                new UsersDto(100, [
                    createUser(12),
                    createUser(13),
                ]));
        }));

    it('should make get request to get single user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            let user: UserDto;

            userManagementService.getUser('123').subscribe(result => {
                user = result;
            });

            const req = httpMock.expectOne('http://service/p/api/user-management/123');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(userResponse(12));

            expect(user!).toEqual(createUser(12));
        }));

    it('should make post request to create user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            const dto = { email: 'mail@squidex.io', displayName: 'Squidex User', permissions: ['Permission1'], password: 'password' };

            let user: UserDto;

            userManagementService.postUser(dto).subscribe(result => {
                user = result;
            });

            const req = httpMock.expectOne('http://service/p/api/user-management');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(userResponse(12));

            expect(user!).toEqual(createUser(12));
        }));

    it('should make put request to update user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            const dto = { email: 'mail@squidex.io', displayName: 'Squidex User', permissions: ['Permission1'], password: 'password' };

            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: 'api/user-management/123' },
                },
            };

            let user: UserDto;

            userManagementService.putUser(resource, dto).subscribe(result => {
                user = result;
            });

            const req = httpMock.expectOne('http://service/p/api/user-management/123');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(userResponse(12));

            expect(user!).toEqual(createUser(12));
        }));

    it('should make put request to lock user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    lock: { method: 'PUT', href: 'api/user-management/123/lock' },
                },
            };

            let user: UserDto;

            userManagementService.lockUser(resource).subscribe(result => {
                user = result;
            });

            const req = httpMock.expectOne('http://service/p/api/user-management/123/lock');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(userResponse(12));

            expect(user!).toEqual(createUser(12));
        }));

    it('should make put request to unlock user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    unlock: { method: 'PUT', href: 'api/user-management/123/unlock' },
                },
            };

            let user: UserDto;

            userManagementService.unlockUser(resource).subscribe(result => {
                user = result;
            });

            const req = httpMock.expectOne('http://service/p/api/user-management/123/unlock');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(userResponse(12));

            expect(user!).toEqual(createUser(12));
        }));

    it('should make delete request to delete user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: 'api/user-management/123' },
                },
            };

            userManagementService.deleteUser(resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/user-management/123');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    function userResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `${id}`,
            email: `user${key}@domain.com`,
            displayName: `user${key}`,
            permissions: [
                `Permission${key}`,
            ],
            isLocked: true,
            _links: {
                update: {
                    method: 'PUT', href: `/users/${id}`,
                },
            },
        };
    }
});

export function createUser(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `/users/${id}` },
    };

    const key = `${id}${suffix}`;

    return new UserDto(links,
        `${id}`,
        `user${key}@domain.com`,
        `user${key}`,
        [
            `Permission${key}`,
        ],
        true);
}
