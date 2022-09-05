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
import { ApiUrlConfig, pretifyError, Resource } from '@app/framework';

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly displayName: string,
    ) {
    }
}

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getUsers(query?: string): Observable<ReadonlyArray<UserDto>> {
        const url = this.apiUrl.buildUrl(`api/users?query=${query || ''}`);

        return this.http.get<any[]>(url).pipe(
            map(body => {
                return parseUsers(body);
            }),
            pretifyError('i18n:users.loadFailed'));
    }

    public getUser(id: string): Observable<UserDto> {
        const url = this.apiUrl.buildUrl(`api/users/${id}`);

        return this.http.get<any>(url).pipe(
            map(body => {
                return parseUser(body);
            }),
            pretifyError('i18n:users.loadUserFailed'));
    }

    public getResources(): Observable<Resource> {
        const url = this.apiUrl.buildUrl('api');

        return this.http.get<Resource>(url).pipe(
            pretifyError('i18n:users.loadUserFailed'));
    }
}

function parseUsers(response: any[]) {
    return response.map(parseUser);
}

function parseUser(response: any) {
    return new UserDto(
        response.id,
        response.displayName);
}

