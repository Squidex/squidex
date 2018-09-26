/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import { ApiUrlConfig } from '@app/framework';

import {
    CreateUserDto,
    UpdateUserDto,
    UserDto,
    UsersDto,
    UsersService
} from './users.service';

describe('UsersService', () => {
     beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                UsersService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
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
                {
                    id: '123',
                    email: 'mail1@domain.com',
                    displayName: 'User1',
                    isLocked: true
                },
                {
                    id: '456',
                    email: 'mail2@domain.com',
                    displayName: 'User2',
                    isLocked: true
                }
            ]
        });

        expect(users!).toEqual(
            new UsersDto(100, [
                new UserDto('123', 'mail1@domain.com', 'User1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', true)
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
                {
                    id: '123',
                    email: 'mail1@domain.com',
                    displayName: 'User1',
                    isLocked: true
                },
                {
                    id: '456',
                    email: 'mail2@domain.com',
                    displayName: 'User2',
                    isLocked: true
                }
            ]
        });

        expect(users!).toEqual(
            new UsersDto(100, [
                new UserDto('123', 'mail1@domain.com', 'User1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', true)
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

        req.flush({
            id: '123',
            email: 'mail1@domain.com',
            displayName: 'User1',
            pictureUrl: 'path/to/image1',
            isLocked: true
        });

        expect(user!).toEqual(new UserDto('123', 'mail1@domain.com', 'User1', true));
    }));

    it('should make post request to create user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {

        const dto = new CreateUserDto('mail@squidex.io', 'Squidex User', 'password');

        let user: UserDto;

        userManagementService.postUser(dto).subscribe(result => {
            user = result;
        });

        const req = httpMock.expectOne('http://service/p/api/user-management');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ id: '123', pictureUrl: 'path/to/image1' });

        expect(user!).toEqual(new UserDto('123', dto.email, dto.displayName, false));
    }));

    it('should make put request to update user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {

        const dto = new UpdateUserDto('mail@squidex.io', 'Squidex User', 'password');

        userManagementService.putUser('123', dto).subscribe();

        const req = httpMock.expectOne('http://service/p/api/user-management/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make put request to lock user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {

        userManagementService.lockUser('123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/user-management/123/lock');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make put request to unlock user',
        inject([UsersService, HttpTestingController], (userManagementService: UsersService, httpMock: HttpTestingController) => {

        userManagementService.unlockUser('123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/user-management/123/unlock');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));
});