/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, UserDto, UsersDto } from '@app/shared';
import { CreateUserDto, IResourceDto, ResourceLinkDto, UpdateUserDto } from '@app/shared/model';
import { UsersService } from './users.service';

describe('UsersService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
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

            const req = httpMock.expectOne('http://service/p/api/user-management?take=20&skip=30');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                total: 100,
                items: [
                    userResponse(12),
                    userResponse(13),
                ],
                _links: {},
            });

            expect(users!).toEqual(
                new UsersDto({
                    total: 100,
                    items: [
                        createUser(12),
                        createUser(13),
                    ],
                    _links: {},
                }));
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
                _links: {},
            });

            expect(users!).toEqual(
                new UsersDto({
                    total: 100,
                    items: [
                        createUser(12),
                        createUser(13),
                    ],
                    _links: {},
                }));
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
            const dto = new CreateUserDto({
                email: 'mail@squidex.io',
                displayName: 'Squidex User',
                permissions: ['Permission1'],
                password: 'password',
            });

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
            const dto = new UpdateUserDto({
                email: 'mail@squidex.io',
                displayName: 'Squidex User',
                permissions: ['Permission1'],
                password: 'password',
            });

            const resource: IResourceDto = {
                _links: {
                    update: new ResourceLinkDto({ method: 'PUT', href: 'api/user-management/123' }),
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
            const resource: IResourceDto = {
                _links: {
                    lock: new ResourceLinkDto({ method: 'PUT', href: 'api/user-management/123/lock' }),
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
            const resource: IResourceDto = {
                _links: {
                    unlock: new ResourceLinkDto({ method: 'PUT', href: 'api/user-management/123/unlock' }),
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
            const resource: IResourceDto = {
                _links: {
                    delete: new ResourceLinkDto({ method: 'DELETE', href: 'api/user-management/123' }),
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
    const key = `${id}${suffix}`;

    return new UserDto({
        id: `${id}`,
        email: `user${key}@domain.com`,
        displayName: `user${key}`,
        permissions: [
            `Permission${key}`,
        ],
        isLocked: true,
        _links: {
            update: new ResourceLinkDto({ method: 'PUT', href: `/users/${id}` }),
        },
    });
}