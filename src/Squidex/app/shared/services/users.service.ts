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
    ResourceLinks
} from '@app/framework';

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly displayName: string
    ) {
    }
}

export class ResourcesDto {
    public readonly _links: ResourceLinks;

    constructor(links: ResourceLinks) {
        this._links = links;
    }
}

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(query?: string): Observable<ReadonlyArray<UserDto>> {
        const url = this.apiUrl.buildUrl(`api/users?query=${query || ''}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                const users = body.map(item =>
                    new UserDto(
                        item.id,
                        item.displayName));

                return users;
            }),
            pretifyError('Failed to load users. Please reload.'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/users/${id}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                const user = new UserDto(
                    body.id,
                    body.displayName);

                return user;
            }),
            pretifyError('Failed to load user. Please reload.'));
    }

    public getResources(): Observable<ResourcesDto> {
        const url = this.apiUrl.buildUrl(`api`);

        return this.http.get<{ _links: {} }>(url).pipe(
            map(({ _links }) => {
                return new ResourcesDto(_links);
            }),
            pretifyError('Failed to load user. Please reload.'));
    }
}