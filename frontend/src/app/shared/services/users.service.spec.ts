/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, Resource, UserDto, UsersService } from '@app/shared/internal';

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

    it('should make post request to update profile',
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {
            usersService.postUser({ answers: {} }).subscribe();

            const req = httpMock.expectOne('http://service/p/api/user');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();
        }));

    it('should make get request to get many users',
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {
            let users: ReadonlyArray<UserDto>;

            usersService.getUsers().subscribe(result => {
                users = result;
            });

            const req = httpMock.expectOne('http://service/p/api/users');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([
                {
                    id: '123',
                    displayName: 'User1',
                },
                {
                    id: '456',
                    displayName: 'User2',
                },
            ]);

            expect(users!).toEqual(
                [
                    new UserDto('123', 'User1'),
                    new UserDto('456', 'User2'),
                ]);
        }));

    it('should make get request with query to get many users',
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {
            let users: ReadonlyArray<UserDto>;

            usersService.getUsers('my-query').subscribe(result => {
                users = result;
            });

            const req = httpMock.expectOne('http://service/p/api/users?query=my-query');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([
                {
                    id: '123',
                    displayName: 'User1',
                },
                {
                    id: '456',
                    displayName: 'User2',
                },
            ]);

            expect(users!).toEqual(
                [
                    new UserDto('123', 'User1'),
                    new UserDto('456', 'User2'),
                ]);
        }));

    it('should make get request to get single user',
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {
            let user: UserDto;

            usersService.getUser('123').subscribe(result => {
                user = result;
            });

            const req = httpMock.expectOne('http://service/p/api/users/123');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({ id: '123', displayName: 'User1' });

            expect(user!).toEqual(new UserDto('123', 'User1'));
        }));

    it('should make get request to get resources',
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {
            let resources: Resource;

            usersService.getResources().subscribe(result => {
                resources = result;
            });

            const req = httpMock.expectOne('http://service/p/api');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                _links: {
                    schemas: { method: 'GET', href: '/api/schemas' },
                },
            });

            expect(resources!).toEqual({
                _links: {
                    schemas: { method: 'GET', href: '/api/schemas' },
                },
            });
        }));
});
