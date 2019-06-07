/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
    ApiUrlConfig,
    pretifyError,
    Resource,
    ResourceLinks,
    ResultSet,
    withLinks
} from '@app/shared';

export class UsersDto  extends ResultSet<UserDto> {
    public readonly _links: ResourceLinks = {};
}

export class UserDto {
    public readonly _links: ResourceLinks = {};

    constructor(
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly permissions: string[] = [],
        public readonly isLocked?: boolean
    ) {
    }
}

export interface CreateUserDto {
    readonly email: string;
    readonly displayName: string;
    readonly permissions: string[];
    readonly password: string;
}

export interface UpdateUserDto {
    readonly email: string;
    readonly displayName: string;
    readonly permissions: string[];
    readonly password?: string;
}

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(take: number, skip: number, query?: string): Observable<UsersDto> {
        const url = this.apiUrl.buildUrl(`api/user-management?take=${take}&skip=${skip}&query=${query || ''}`);

        return this.http.get<{ total: number, items: any[] } & Resource>(url).pipe(
                map(body => {
                    const users = body.items.map(item => parseUser(item));

                    return withLinks(new UsersDto(body.total, users), body);
                }),
                pretifyError('Failed to load users. Please reload.'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return this.http.get<any>(url).pipe(
                map(body => {
                    return parseUser(body);
                }),
                pretifyError('Failed to load user. Please reload.'));
    }

    public postUser(dto: CreateUserDto): Observable<UserDto> {
        const url = this.apiUrl.buildUrl('api/user-management');

        return this.http.post<any>(url, dto).pipe(
                map(body => {
                    return parseUser(body);
                }),
                pretifyError('Failed to create user. Please reload.'));
    }

    public putUser(user: Resource, dto: UpdateUserDto): Observable<UserDto> {
        const link = user._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, { body: dto }).pipe(
                map(body => {
                    return parseUser(body);
                }),
                pretifyError('Failed to update user. Please reload.'));
    }

    public lockUser(user: Resource): Observable<UserDto> {
        const link = user._links['lock'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
                map(body => {
                    return parseUser(body);
                }),
                pretifyError('Failed to load users. Please retry.'));
    }

    public unlockUser(user: Resource): Observable<UserDto> {
        const link = user._links['unlock'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
                map(body => {
                    return parseUser(body);
                }),
                pretifyError('Failed to load users. Please retry.'));
    }
}

function parseUser(response: any) {
    return withLinks(
        new UserDto(
            response.id,
            response.email,
            response.displayName,
            response.permissions,
            response.isLocked),
        response);
}