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
    Model,
    pretifyError,
    Resource,
    ResourceLinks,
    ResultSet,
    withLinks
} from '@app/shared';

export class UsersDto  extends ResultSet<UserDto> {
    public _links: ResourceLinks;
}

export class UserDto extends Model<UserDto> {
    public _links: ResourceLinks;

    constructor(
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly permissions: string[] = [],
        public readonly isLocked?: boolean
    ) {
        super();
    }

    public with(value: Partial<UserDto>): UserDto {
        return this.clone(value);
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
                    const users = body.items.map(item =>
                        withLinks(
                            new UserDto(
                                item.id,
                                item.email,
                                item.displayName,
                                item.permissions,
                                item.isLocked),
                            item));

                    return withLinks(new UsersDto(body.total, users), body);
                }),
                pretifyError('Failed to load users. Please reload.'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return this.http.get<any>(url).pipe(
                map(body => {
                    const user = new UserDto(
                        body.id,
                        body.email,
                        body.displayName,
                        body.permissions,
                        body.isLocked);

                    return user;
                }),
                pretifyError('Failed to load user. Please reload.'));
    }

    public postUser(dto: CreateUserDto): Observable<UserDto> {
        const url = this.apiUrl.buildUrl('api/user-management');

        return this.http.post<any>(url, dto).pipe(
                map(body => {
                    const user = new UserDto(
                        body.id,
                        dto.email,
                        dto.displayName,
                        dto.permissions,
                        false);

                    return user;
                }),
                pretifyError('Failed to create user. Please reload.'));
    }

    public putUser(id: string, dto: UpdateUserDto): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return this.http.put(url, dto).pipe(
                pretifyError('Failed to update user. Please reload.'));
    }

    public lockUser(id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}/lock`);

        return this.http.put(url, {}).pipe(
                pretifyError('Failed to load users. Please retry.'));
    }

    public unlockUser(id: string): Observable<any> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}/unlock`);

        return this.http.put(url, {}).pipe(
                pretifyError('Failed to load users. Please retry.'));
    }
}