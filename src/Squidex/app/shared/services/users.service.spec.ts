/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';

import {
    ApiUrlConfig,
    UserDto,
    UsersService
} from './../';

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