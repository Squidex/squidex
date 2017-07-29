/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    CreateUserDto,
    UpdateUserDto,
    UserDto,
    UserManagementService,
    UsersDto,
    UsersService
} from './../';

describe('UserDto', () => {
    it('should update email and display name property when unlocking', () => {
        const user_1 = new UserDto('1', 'sebastian@squidex.io', 'Sebastian', 'picture', true);
        const user_2 = user_1.update('qaisar@squidex.io', 'Qaisar');

        expect(user_2.email).toEqual('qaisar@squidex.io');
        expect(user_2.displayName).toEqual('Qaisar');
    });

    it('should update isLocked property when locking', () => {
        const user_1 = new UserDto('1', 'sebastian@squidex.io', 'Sebastian', 'picture', false);
        const user_2 = user_1.lock();

        expect(user_2.isLocked).toBeTruthy();
    });

    it('should update isLocked property when unlocking', () => {
        const user_1 = new UserDto('1', 'sebastian@squidex.io', 'Sebastian', 'picture', true);
        const user_2 = user_1.unlock();

        expect(user_2.isLocked).toBeFalsy();
    });
});

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
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {

        let users: UserDto[] | null = null;

        usersService.getUsers().subscribe(result => {
            users = result;
        });

        const req = httpMock.expectOne('http://service/p/api/users?query=');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                id: '123',
                email: 'mail1@domain.com',
                displayName: 'User1',
                pictureUrl: 'path/to/image1',
                isLocked: true
            },
            {
                id: '456',
                email: 'mail2@domain.com',
                displayName: 'User2',
                pictureUrl: 'path/to/image2',
                isLocked: true
            }
        ]);

        expect(users).toEqual(
            [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
            ]);
    }));

    it('should make get request with query to get many users',
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {

        let users: UserDto[] | null = null;

        usersService.getUsers('my-query').subscribe(result => {
            users = result;
        });

        const req = httpMock.expectOne('http://service/p/api/users?query=my-query');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush([
            {
                id: '123',
                email: 'mail1@domain.com',
                displayName: 'User1',
                pictureUrl: 'path/to/image1',
                isLocked: true
            },
            {
                id: '456',
                email: 'mail2@domain.com',
                displayName: 'User2',
                pictureUrl: 'path/to/image2',
                isLocked: true
            }
        ]);

        expect(users).toEqual(
            [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
            ]);
    }));

    it('should make get request to get single user',
        inject([UsersService, HttpTestingController], (usersService: UsersService, httpMock: HttpTestingController) => {

        let user: UserDto | null = null;

        usersService.getUser('123').subscribe(result => {
            user = result;
        });

        const req = httpMock.expectOne('http://service/p/api/users/123');

        expect(req.request.method).toEqual('GET');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({
            id: '123',
            email: 'mail1@domain.com',
            displayName: 'User1',
            pictureUrl: 'path/to/image1',
            isLocked: true
        });

        expect(user).toEqual(new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true));
    }));
});

describe('UserManagementService', () => {
     beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                UserManagementService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') }
            ]
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get many users',
        inject([UserManagementService, HttpTestingController], (userManagementService: UserManagementService, httpMock: HttpTestingController) => {

        let users: UsersDto | null = null;

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
                    pictureUrl: 'path/to/image1',
                    isLocked: true
                },
                {
                    id: '456',
                    email: 'mail2@domain.com',
                    displayName: 'User2',
                    pictureUrl: 'path/to/image2',
                    isLocked: true
                }
            ]
        });

        expect(users).toEqual(
            new UsersDto(100, [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
            ]));
    }));

    it('should make get request with query to get many users',
        inject([UserManagementService, HttpTestingController], (userManagementService: UserManagementService, httpMock: HttpTestingController) => {

        let users: UsersDto | null = null;

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
                    pictureUrl: 'path/to/image1',
                    isLocked: true
                },
                {
                    id: '456',
                    email: 'mail2@domain.com',
                    displayName: 'User2',
                    pictureUrl: 'path/to/image2',
                    isLocked: true
                }
            ]
        });

        expect(users).toEqual(
            new UsersDto(100, [
                new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true),
                new UserDto('456', 'mail2@domain.com', 'User2', 'path/to/image2', true)
            ]));
    }));

    it('should make get request to get single user',
        inject([UserManagementService, HttpTestingController], (userManagementService: UserManagementService, httpMock: HttpTestingController) => {

        let user: UserDto | null = null;

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

        expect(user).toEqual(new UserDto('123', 'mail1@domain.com', 'User1', 'path/to/image1', true));
    }));

    it('should make post request to create user',
        inject([UserManagementService, HttpTestingController], (userManagementService: UserManagementService, httpMock: HttpTestingController) => {

        const dto = new CreateUserDto('mail@squidex.io', 'Squidex User', 'password');

        let user: UserDto | null = null;

        userManagementService.postUser(dto).subscribe(result => {
            user = result;
        });

        const req = httpMock.expectOne('http://service/p/api/user-management');

        expect(req.request.method).toEqual('POST');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({ id: '123', pictureUrl: 'path/to/image1' });

        expect(user).toEqual(new UserDto('123', dto.email, dto.displayName, 'path/to/image1', false));
    }));

    it('should make put request to update user',
        inject([UserManagementService, HttpTestingController], (userManagementService: UserManagementService, httpMock: HttpTestingController) => {

        const dto = new UpdateUserDto('mail@squidex.io', 'Squidex User', 'password');

        userManagementService.putUser('123', dto).subscribe();

        const req = httpMock.expectOne('http://service/p/api/user-management/123');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make put request to lock user',
        inject([UserManagementService, HttpTestingController], (userManagementService: UserManagementService, httpMock: HttpTestingController) => {

        userManagementService.lockUser('123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/user-management/123/lock');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));

    it('should make put request to unlock user',
        inject([UserManagementService, HttpTestingController], (userManagementService: UserManagementService, httpMock: HttpTestingController) => {

        userManagementService.unlockUser('123').subscribe();

        const req = httpMock.expectOne('http://service/p/api/user-management/123/unlock');

        expect(req.request.method).toEqual('PUT');
        expect(req.request.headers.get('If-Match')).toBeNull();

        req.flush({});
    }));
});