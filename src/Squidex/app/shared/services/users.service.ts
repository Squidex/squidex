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

import { ApiUrlConfig, pretifyError } from '@app/framework';

export class UserDto {
    constructor(
        public readonly id: string,
        public readonly displayName: string,
        public readonly email: string
    ) {
    }
}

@Injectable()
export class UsersService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public getUsers(query?: string): Observable<UserDto[]> {
        const url = this.apiUrl.buildUrl(`api/users?query=${query || ''}`);

        return this.http.get<any[]>(url).pipe(
                map(body => {
                    const users = body.map(item =>
                        new UserDto(
                            item.id,
                            item.displayName,
                            item.email));

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
                        body.displayName,
                        body.email);

                    return user;
                }),
                pretifyError('Failed to load user. Please reload.'));
    }
}