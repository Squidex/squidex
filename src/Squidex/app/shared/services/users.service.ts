/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import 'framework/angular/http-extensions';

import { ApiUrlConfig } from 'framework';
import { AuthService } from './auth.service';

export class UsersDto {
    constructor(
        public readonly total: number,
        public readonly items: UserDto[]
    ) {
    }
}

export class UserCreatedDto {
    constructor(
        public readonly id: string,
        public readonly pictureUrl: string
    ) {
    }
}

export class CreateUserDto {
    constructor(
        public readonly email: string,
        public readonly displayName: string,
        public readonly password: string
    ) {
    }
}

export class UpdateUserDto {
    constructor(
        public readonly email: string,
        public readonly displayName: string,
        public readonly password: string
    ) {
    }
}

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly pictureUrl: string | null,
        public readonly isLocked: boolean
    ) {
    }
}

@Injectable()
export class UsersService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(query?: string): Observable<UserDto[]> {
        const url = this.apiUrl.buildUrl(`api/users?query=${query || ''}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response;

                    return items.map(item => {
                        return new UserDto(
                            item.id,
                            item.email,
                            item.displayName,
                            item.pictureUrl,
                            item.isLocked);
                    });
                })
                .catchError('Failed to load users. Please reload.');
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/users/${id}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    return new UserDto(
                        response.id,
                        response.email,
                        response.displayName,
                        response.pictureUrl,
                        response.isLocked);
                })
                .catchError('Failed to load user. Please reload.');
    }
}

@Injectable()
export class UserManagementService {
    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(take: number, skip: number, query?: string): Observable<UsersDto> {
        const url = this.apiUrl.buildUrl(`api/user-management?take=${take}&skip=${skip}&query=${query || ''}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    const items: any[] = response.items;

                    const users = items.map(item => {
                        return new UserDto(
                            item.id,
                            item.email,
                            item.displayName,
                            item.pictureUrl,
                            item.isLocked);
                    });

                    return new UsersDto(response.total, users);
                })
                .catchError('Failed to load users. Please reload.');
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return this.authService.authGet(url)
                .map(response => response.json())
                .map(response => {
                    return new UserDto(
                        response.id,
                        response.email,
                        response.displayName,
                        response.pictureUrl,
                        response.isLocked);
                })
                .catchError('Failed to load user. Please reload.');
    }

    public postUser(dto: CreateUserDto): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/user-management/`);

        return this.authService.authPost(url, dto)
                .map(response => response.json())
                .map(response => new UserCreatedDto(response.id, response.pictureUrl))
                .catchError('Failed to create user. Please reload.');
    }

    public putUser(id: string, dto: UpdateUserDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return this.authService.authPut(url, dto)
                .catchError('Failed to update user. Please reload.');
    }

    public lockUser(id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}/lock`);

        return this.authService.authPut(url, {})
                .catchError('Failed to load users. Please retry.');
    }

    public unlockUser(id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}/unlock`);

        return this.authService.authPut(url, {})
                .catchError('Failed to load users. Please retry.');
    }
}