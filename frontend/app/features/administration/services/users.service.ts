/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiUrlConfig, hasAnyLink, pretifyError, Resource, ResourceLinks, ResultSet } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export class UsersDto extends ResultSet<UserDto> {
    public get canCreate() {
        return hasAnyLink(this._links, 'create');
    }
}

export class UserDto {
    public readonly _links: ResourceLinks;

    public readonly canLock: boolean;
    public readonly canUnlock: boolean;
    public readonly canUpdate: boolean;
    public readonly canDelete: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly permissions: ReadonlyArray<string> = [],
        public readonly isLocked?: boolean,
    ) {
        this._links = links;

        this.canLock = hasAnyLink(links, 'lock');
        this.canUnlock = hasAnyLink(links, 'unlock');
        this.canUpdate = hasAnyLink(links, 'update');
        this.canDelete = hasAnyLink(links, 'delete');
    }
}

type Permissions = readonly string[];

export type CreateUserDto =
    Readonly<{ email: string; displayName: string; permissions: Permissions; password: string }>;

export type UpdateUserDto =
    Partial<CreateUserDto>;

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getUsers(take: number, skip: number, query?: string): Observable<UsersDto> {
        const url = this.apiUrl.buildUrl(`api/user-management?take=${take}&skip=${skip}&query=${query || ''}`);

        return this.http.get<{ total: number; items: any[] } & Resource>(url).pipe(
            map(({ total, items, _links }) => {
                const users = items.map(parseUser);

                return new UsersDto(total, users, _links);
            }),
            pretifyError('i18n:users.loadFailed'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/user-management/${id}`);

        return this.http.get(url).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.loadUserFailed'));
    }

    public postUser(dto: CreateUserDto): Observable<UserDto> {
        const url = this.apiUrl.buildUrl('api/user-management');

        return this.http.post(url, dto).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.createFailed'));
    }

    public putUser(user: Resource, dto: UpdateUserDto): Observable<UserDto> {
        const link = user._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url, { body: dto }).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.updateFailed'));
    }

    public lockUser(user: Resource): Observable<UserDto> {
        const link = user._links['lock'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.lockFailed'));
    }

    public unlockUser(user: Resource): Observable<UserDto> {
        const link = user._links['unlock'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.unlockFailed'));
    }

    public deleteUser(user: Resource): Observable<any> {
        const link = user._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return this.http.request(link.method, url).pipe(
            pretifyError('i18n:users.deleteFailed'));
    }
}

function parseUser(response: any) {
    return new UserDto(
        response._links,
        response.id,
        response.email,
        response.displayName,
        response.permissions,
        response.isLocked);
}
