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
import { ApiUrlConfig, hasAnyLink, pretifyError, Resource, ResourceLinks } from '@app/shared';

export class UserDto implements Resource {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canLock: boolean;
    public readonly canUnlock: boolean;
    public readonly canUpdate: boolean;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly email: string,
        public readonly displayName: string,
        public readonly permissions: ReadonlyArray<string> = [],
        public readonly isLocked?: boolean,
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canLock = hasAnyLink(links, 'lock');
        this.canUnlock = hasAnyLink(links, 'unlock');
        this.canUpdate = hasAnyLink(links, 'update');
    }
}

export type UsersDto = Readonly<{
    // The list of users.
    items: ReadonlyArray<UserDto>;

    // The number of users.
    total: number;

    // True, if the user has permissions to create a user.
    canCreate?: boolean;
}>;

export type UpsertUserDto = Readonly<{
    // The email address of the user.
    email: string;

    // The display name.
    displayName?: string;

    // The permissions as in the dot-notation.
    permissions?: ReadonlyArray<string>;

    // The password (confirm is only used in the UI).
    password?: string;
}>;

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getUsers(take: number, skip: number, query?: string): Observable<UsersDto> {
        const url = this.apiUrl.buildUrl(`api/user-management?take=${take}&skip=${skip}&query=${query || ''}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseUsers(body);
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

    public postUser(dto: UpsertUserDto): Observable<UserDto> {
        const url = this.apiUrl.buildUrl('api/user-management');

        return this.http.post(url, dto).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.createFailed'));
    }

    public putUser(user: Resource, dto: Partial<UpsertUserDto>): Observable<UserDto> {
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

function parseUsers(response: { items: any[]; total: number } & Resource): UsersDto {
    const { items: list, total, _links } = response;
    const items = list.map(parseUser);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, total, canCreate };
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
